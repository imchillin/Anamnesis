// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Diagnostics;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis;
	using Anamnesis.Core.Memory;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Memory;
	using Anamnesis.Memory.Marshalers;

	public delegate void SelectionModeEvent(Modes mode);
	public delegate void SelectionEvent(Actor actor);

	public enum Modes
	{
		Overworld,
		GPose,
	}

	public class TargetService : IService
	{
		private static IMarshaler<bool>? gposeMem;
		private static IMarshaler<ushort>? gposeMem2;

		public static event SelectionModeEvent? ModeChanged;
		public static event SelectionEvent? ActorSelected;

		public static Actor? SelectedActor { get; private set; }

		public static Anamnesis.Memory.Actor? CurrentTarget
		{
			get => GetActorByOffset(0x80);
			set => SetActorByOffset(0x80, value);
		}

		public static Modes CurrentMode
		{
			get
			{
				if (gposeMem == null || gposeMem2 == null)
					throw new Exception("Unable to get selection mode");

				return gposeMem.Value && gposeMem2.Value == 4 ? Modes.GPose : Modes.Overworld;
			}
		}

		public bool IsAlive
		{
			get;
			private set;
		}

		public Task Initialize()
		{
			this.IsAlive = true;
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			this.IsAlive = false;
			return Task.CompletedTask;
		}

		public Task Start()
		{
			gposeMem = MemoryService.GetMarshaler(Offsets.Main.GposeCheck);
			gposeMem2 = MemoryService.GetMarshaler(Offsets.Main.GposeCheck2);

			Task.Run(this.Watch);

			return Task.CompletedTask;
		}

		public async Task SelectActor(Actor actor)
		{
			SelectedActor = actor;

			using IMarshaler<int> territoryMem = MemoryService.GetMarshaler(Offsets.Main.TerritoryAddress, Offsets.Main.Territory);

			int territoryId = territoryMem.Value;

			bool isBarracks = false;
			isBarracks |= territoryId == 534; // Twin adder barracks
			isBarracks |= territoryId == 535; // Immortal Flame barracks
			isBarracks |= territoryId == 536; // Maelstrom barracks

			// Mannequins and housing NPC's get actor type changed, but squadron members do not.
			if (!isBarracks && actor.Type == ActorTypes.EventNpc)
			{
				bool? result = await GenericDialog.Show($"The Actor: \"{actor.Name}\" appears to be a humanoid NPC. Do you want to change them to a player to allow for posing and appearance changes?", "Actor Selection", MessageBoxButton.YesNo);

				if (result == null)
					return;

				if (result == true)
				{
					actor.SetValue(Offsets.Main.ActorType, ActorTypes.Player);
					actor.Type = ActorTypes.Player;
					await actor.ActorRefreshAsync();

					if (actor.GetValue(Offsets.Main.ModelType) != 0)
					{
						actor.SetValue(Offsets.Main.ModelType, 0);
						await actor.ActorRefreshAsync();
					}
				}
			}

			// Carbuncles get model type set to player (but not actor type!)
			if (actor.Type == ActorTypes.BattleNpc)
			{
				int modelType = actor.GetValue(Offsets.Main.ModelType);
				if (modelType == 409 || modelType == 410 || modelType == 412)
				{
					bool? result = await GenericDialog.Show($"The Actor: \"{actor.Name}\" appears to be a Carbuncle. Do you want to change them to a player to allow for posing and appearance changes?", "Actor Selection", MessageBoxButton.YesNo);

					if (result == null)
						return;

					if (result == true)
					{
						actor.SetValue(Offsets.Main.ModelType, 0);
						await actor.ActorRefreshAsync();
					}
				}
			}

			ActorSelected?.Invoke(actor);
		}

		private static Anamnesis.Memory.Actor? GetActorByOffset(int offset)
		{
			IntPtr address = AddressService.TargetManager + offset;
			IntPtr ptr = MemoryService.ReadPtr(address);

			return MemoryService.Read<Anamnesis.Memory.Actor>(ptr);
		}

		private static void SetActorByOffset(int offset, Anamnesis.Memory.Actor? actor)
		{
			throw new NotImplementedException();
		}

		private async Task Watch()
		{
			try
			{
				await Task.Delay(500);

				IActorRefreshService refreshService = Services.Get<IActorRefreshService>();

				Modes currentMode = CurrentMode;
				Anamnesis.Memory.Actor? lastTarget = CurrentTarget;

				while (this.IsAlive)
				{
					await Task.Delay(50);

					while (refreshService.IsRefreshing)
						await Task.Delay(250);

					Modes newMode = CurrentMode;

					if (newMode != currentMode)
					{
						await Task.Delay(1000);

						currentMode = newMode;

						try
						{
							ModeChanged?.Invoke(newMode);
						}
						catch (Exception ex)
						{
							Log.Write(ex, "Selection", Log.Severity.Error);
						}
					}

					Anamnesis.Memory.Actor? newTarget = CurrentTarget;
					if (!Anamnesis.Memory.Actor.IsSame(newTarget, lastTarget))
					{
						lastTarget = newTarget;

						try
						{
							Actor ac = new Actor(Offsets.Main.Target);
							await this.SelectActor(ac);
						}
						catch (Exception ex)
						{
							Log.Write(new Exception("Failed to select current target", ex), "Selection", Log.Severity.Warning);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
