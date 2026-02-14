// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Refresh;

using Anamnesis.Files;
using Anamnesis.Memory;
using Serilog;
using System;
using System.Threading.Tasks;

/// <summary>
/// Orchestrates character appearance changes using a snapshot-based approach.
/// Captures current state, applies target state, computes diff, and performs optimal redraw.
/// </summary>
public class ActorAppearanceService
{
	private static ActorAppearanceService? s_instance;

	/// <summary>Event raised before and after applying appearance changes.</summary>
	public event Action<ActorMemory, RedrawStage>? OnAppearanceChanged;

	/// <summary>Gets the singleton instance.</summary>
	public static ActorAppearanceService Instance => s_instance ??= new ActorAppearanceService();

	/// <summary>
	/// Applies a character file to an actor, computing the optimal redraw strategy.
	/// </summary>
	/// <param name="handle">A handle of the actor to modify.</param>
	/// <param name="targetAppearance">The character file containing target appearance.</param>
	/// <param name="mode">Which sections to apply.</param>
	/// <returns>The changeset describing what was modified.</returns>
	public async Task<CharacterFile.CharChangeSet> ApplyAppearanceAsync(
		ObjectHandle<ActorMemory> handle,
		CharacterFile targetAppearance,
		CharacterFile.SaveModes mode = CharacterFile.SaveModes.All)
	{
		CharacterFile.CharChangeSet changeset = CharacterFile.CharChangeSet.None;

		await handle.DoAsync(async a =>
		{
			if (!a.CanRefresh || a.Address == IntPtr.Zero)
			{
				Log.Warning($"Actor at 0x{a.Address:X} cannot be refreshed");
				changeset = CharacterFile.CharChangeSet.None;
				return;
			}

			// 1. Capture current state as snapshot
			var currentSnapshot = new CharacterFile();
			currentSnapshot.WriteToFile(handle, mode);

			// 2. Compute what will change
			changeset = currentSnapshot.CompareTo(targetAppearance, mode);
			Log.Information($"Computed changeset: {changeset}");

			if (!changeset.HasChanges)
			{
				Log.Information("No changes detected, skipping redraw");
				return;
			}

			// 3. Apply target appearance to memory
			bool prevAutoRefresh = a.AutomaticRefreshEnabled;
			try
			{
				a.AutomaticRefreshEnabled = false;
				a.PauseSynchronization = true;

				// Apply changes (this reuses existing CharacterFile.Apply logic, but we control the redraw)
				await ApplyToMemoryAsync(handle, targetAppearance, mode);

				a.PauseSynchronization = false;
				a.Synchronize(exclGroups: TargetService.ExcludeSkeletonGroup);
			}
			finally
			{
				a.AutomaticRefreshEnabled = prevAutoRefresh;
				a.PauseSynchronization = false;
			}

			// 4. Perform optimal redraw based on changeset
			await this.RedrawAsync(a, changeset);

			// 5. Apply extended appearance (doesn't require redraw)
			ApplyExtendedAppearance(a, targetAppearance, mode);
		});

		return changeset;
	}

	/// <summary>
	/// Performs a redraw using the optimal strategy based on the changeset.
	/// </summary>
	public async Task RedrawAsync(ActorMemory actor, CharacterFile.CharChangeSet changeset)
	{
		if (!actor.CanRefresh || actor.Address == IntPtr.Zero)
			return;

		if (!changeset.HasChanges)
			return;

		this.OnAppearanceChanged?.Invoke(actor, RedrawStage.Before);

		try
		{
			if (changeset.CanUseOptimizedRedraw && actor.IsHuman)
			{
				var drawData = actor.BuildDrawData();
				bool skipEquipment = !changeset.HasEquipmentChanges && !changeset.HasWeaponChanges;

				if (actor.UpdateDrawData(in drawData, skipEquipment))
				{
					Log.Information("Applied changes via optimized UpdateDrawData");
					this.OnAppearanceChanged?.Invoke(actor, RedrawStage.After);
					return;
				}

				Log.Information("Optimized UpdateDrawData failed, falling back to full redraw");
			}

			Log.Information($"Performing full redraw (Structural: {changeset.HasStructuralChanges}, IsHuman: {actor.IsHuman})");
			await ActorMemory.RefreshActor(actor);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Error during redraw for actor at 0x{actor.Address:X}");
		}

		this.OnAppearanceChanged?.Invoke(actor, RedrawStage.After);
	}

	/// <summary>
	/// Forces a full redraw without computing changes.
	/// </summary>
	public Task ForceRedrawAsync(ActorMemory actor)
		=> this.RedrawAsync(actor, CharacterFile.CharChangeSet.FullRedraw);

	private static Task ApplyToMemoryAsync(ObjectHandle<ActorMemory> handle, CharacterFile file, CharacterFile.SaveModes mode)
	{
		// Reuse existing write logic from CharacterFile
		// This applies values to memory without triggering redraw
		file.Apply(handle, mode);
		return Task.CompletedTask;
	}

	private static void ApplyExtendedAppearance(ActorMemory actor, CharacterFile file, CharacterFile.SaveModes mode)
	{
		if (actor.ModelObject?.ExtendedAppearance == null)
			return;

		if (mode.HasFlag(CharacterFile.SaveModes.AppearanceHair))
		{
			actor.ModelObject.ExtendedAppearance.HairColor = file.HairColor ?? actor.ModelObject.ExtendedAppearance.HairColor;
			actor.ModelObject.ExtendedAppearance.HairGloss = file.HairGloss ?? actor.ModelObject.ExtendedAppearance.HairGloss;
			actor.ModelObject.ExtendedAppearance.HairHighlight = file.HairHighlight ?? actor.ModelObject.ExtendedAppearance.HairHighlight;
		}

		if (mode.HasFlag(CharacterFile.SaveModes.AppearanceFace))
		{
			actor.ModelObject.ExtendedAppearance.LeftEyeColor = file.LeftEyeColor ?? actor.ModelObject.ExtendedAppearance.LeftEyeColor;
			actor.ModelObject.ExtendedAppearance.RightEyeColor = file.RightEyeColor ?? actor.ModelObject.ExtendedAppearance.RightEyeColor;
			actor.ModelObject.ExtendedAppearance.LimbalRingColor = file.LimbalRingColor ?? actor.ModelObject.ExtendedAppearance.LimbalRingColor;
			actor.ModelObject.ExtendedAppearance.MouthColor = file.MouthColor ?? actor.ModelObject.ExtendedAppearance.MouthColor;
		}

		if (mode.HasFlag(CharacterFile.SaveModes.AppearanceBody))
		{
			actor.ModelObject.ExtendedAppearance.SkinColor = file.SkinColor ?? actor.ModelObject.ExtendedAppearance.SkinColor;
			actor.ModelObject.ExtendedAppearance.SkinGloss = file.SkinGloss ?? actor.ModelObject.ExtendedAppearance.SkinGloss;
			actor.ModelObject.ExtendedAppearance.MuscleTone = file.MuscleTone ?? actor.ModelObject.ExtendedAppearance.MuscleTone;
			actor.Transparency = file.Transparency ?? actor.Transparency;

			if (file.HeightMultiplier.HasValue)
				actor.ModelObject.Height = file.HeightMultiplier.Value;

			if (actor.ModelObject.Bust?.Scale != null && file.BustScale.HasValue)
				actor.ModelObject.Bust.Scale = file.BustScale.Value;
		}
	}
}