// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A service that manages the animation state of actors in the game
/// (e.g., applying animations, animation blending, speed control, etc.).
/// </summary>
[AddINotifyPropertyChangedInterface]
public class AnimationService : ServiceBase<AnimationService>
{
	private const int TICK_DELAY = 1000;
	private const ushort DRAW_WEAPON_ANIMATION_ID = 34;
	private const ushort IDLE_ANIMATION_ID = 3;

	private readonly HashSet<ActorMemory> overriddenActors = [];

	private NopHook? animationSpeedHook;
	private bool speedControlEnabled = false;

	/// <summary>
	/// Gets or sets a value indicating whether the animation speed control is enabled.
	/// </summary>
	/// <remarks>
	/// Changes to this property will be applied only if the player is in GPose mode.
	/// </remarks>
	public bool SpeedControlEnabled
	{
		get => this.speedControlEnabled;
		set
		{
			if (this.speedControlEnabled != value)
				this.SetSpeedControlEnabled(value && GposeService.Instance.IsGpose);
		}
	}

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies =>
	[
		ActorService.Instance,
		TargetService.Instance,
		GposeService.Instance,
		GameDataService.Instance
	];

	/// <summary>
	/// Blends the specified animation to the actor's current animation.
	/// </summary>
	/// <param name="memory">The actor's memory.</param>
	/// <param name="animationId">The animation ID to blend.</param>
	/// <returns>True if the animation was successfully blended; otherwise, false.</returns>
	public static async Task<bool> BlendAnimation(ActorMemory memory, ushort animationId)
	{
		if (!memory.IsValid)
			return false;

		if (memory.Animation!.BlendLocked)
			return false;

		if (!memory.CanAnimate)
			return false;

		memory.Animation!.BlendLocked = true;

		ushort oldAnim = memory.Animation!.BaseOverride;
		memory.Animation!.BaseOverride = animationId;
		await Task.Delay(66);
		memory.Animation!.BaseOverride = oldAnim;

		memory.Animation!.BlendLocked = false;

		return true;
	}

	/// <inheritdoc/>
	public override async Task Shutdown()
	{
		GposeService.GposeStateChanged -= this.OnGposeStateChanged;
		this.SpeedControlEnabled = false;
		this.ResetOverriddenActors();
		await base.Shutdown();
	}

	/// <summary>
	/// Applies an animation override to the specified actor.
	/// </summary>
	/// <param name="memory">The actor's memory.</param>
	/// <param name="animationId">The animation ID to apply.</param>
	/// <param name="interrupt">A flag indicating whether to interrupt the currently played animation.</param>
	/// <returns>True if the animation override was successfully applied; otherwise, false.</returns>
	public bool ApplyAnimationOverride(ActorMemory memory, ushort? animationId, bool interrupt)
	{
		if (!memory.IsValid)
			return false;

		if (!memory.CanAnimate)
			return false;

		ApplyBaseAnimationInternal(memory, animationId, interrupt, ActorMemory.CharacterModes.AnimLock, 0);
		this.overriddenActors.Add(memory);

		return true;
	}

	/// <summary>
	/// Resets the animation override for the specified actor.
	/// </summary>
	/// <param name="memory">The actor's memory.</param>
	public void ResetAnimationOverride(ActorMemory memory)
	{
		if (!memory.IsValid)
			return;

		ApplyBaseAnimationInternal(memory, 0, true, ActorMemory.CharacterModes.Normal, 0);

		AnimationMemory animation = memory.Animation!;

		animation.LipsOverride = 0;
		animation.LinkSpeeds = true;
		animation.Speeds![(int)AnimationMemory.AnimationSlots.FullBody].Value = 1.0f;

		this.overriddenActors.Remove(memory);
	}

	/// <summary>
	/// Applies the idle animation override to the specified actor.
	/// </summary>
	/// <param name="memory">The actor's memory.</param>
	public void ApplyIdle(ActorMemory memory) => this.ApplyAnimationOverride(memory, IDLE_ANIMATION_ID, true);

	/// <summary>
	/// Applies the draw weapon animation override to the specified actor.
	/// </summary>
	/// <param name="memory">The actor's memory.</param>
	public void DrawWeapon(ActorMemory memory) => this.ApplyAnimationOverride(memory, DRAW_WEAPON_ANIMATION_ID, true);

	/// <summary>
	/// Pauses the animation of all pinned actors. This will set their animation speed to 0.
	/// </summary>
	public void PausePinnedActors()
	{
		this.SpeedControlEnabled = true;

		if (this.SpeedControlEnabled)
		{
			var actors = TargetService.Instance.PinnedActors.ToList();
			foreach (var actor in actors)
			{
				if (actor.IsValid && actor.Memory != null && actor.Memory.Address != IntPtr.Zero && actor.Memory.IsValid)
				{
					actor.Memory.Do(a => a.Animation!.LinkSpeeds = true);
					actor.Memory.Do(a => a.Animation!.Speeds![(int)AnimationMemory.AnimationSlots.FullBody].Value = 0.0f);
				}
			}
		}
	}

	/// <inheritdoc/>
	protected override Task OnStart()
	{
		GposeService.GposeStateChanged += this.OnGposeStateChanged;

		this.animationSpeedHook = new NopHook(AddressService.AnimationSpeedPatch, 0x9);

		this.OnGposeStateChanged(GposeService.Instance.IsGpose);

		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.CheckThread(this.CancellationToken));
		return base.OnStart();
	}

	private static void ApplyBaseAnimationInternal(ActorMemory memory, ushort? animationId, bool interrupt, ActorMemory.CharacterModes? mode, byte? modeInput)
	{
		if (animationId != null && memory.Animation!.BaseOverride != animationId)
		{
			if (animationId < GameDataService.ActionTimelines.Count)
			{
				memory.Animation!.BaseOverride = (ushort)animationId;
			}
		}

		// Always set the input before the mode
		if (modeInput != null && memory.CharacterModeInput != modeInput)
		{
			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterModeInput)), modeInput);
		}

		if (mode != null && memory.CharacterMode != mode)
		{
			MemoryService.Write(memory.GetAddressOfProperty(nameof(ActorMemory.CharacterModeRaw)), mode);
		}

		if (interrupt)
		{
			memory.Animation!.AnimationIds![(int)AnimationMemory.AnimationSlots.FullBody].Value = 0;
		}

		memory.Synchronize();
	}

	private async Task CheckThread(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				this.CleanupInvalidActors();
				await Task.Delay(TICK_DELAY, cancellationToken);
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop
				break;
			}
		}
	}

	private void SetSpeedControlEnabled(bool enabled)
	{
		if (this.speedControlEnabled == enabled)
			return;

		if (enabled)
		{
			this.animationSpeedHook?.SetEnabled(true);
		}
		else
		{
			this.animationSpeedHook?.SetEnabled(false);
		}

		this.speedControlEnabled = enabled;
	}

	private void OnGposeStateChanged(bool isGPose)
	{
		if (!isGPose)
			this.SpeedControlEnabled = false;
	}

	private void ResetOverriddenActors()
	{
		foreach (var actor in this.overriddenActors.ToList())
		{
			if (actor.IsValid && actor.IsAnimationOverridden)
			{
				this.ResetAnimationOverride(actor);
			}
		}
	}

	private void CleanupInvalidActors()
	{
		var stale = this.overriddenActors.Where(actor => !actor.IsValid).ToList();
		foreach (var actorRef in stale)
		{
			this.overriddenActors.Remove(actorRef);
		}
	}
}
