// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Windows.Documents;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.WpfStyles;
	using FontAwesome.Sharp;
	using SimpleLog;

	public delegate void SelectionEvent(ActorViewModel? actor);

	public class TargetService : ServiceBase<TargetService>
	{
		public static event SelectionEvent? ActorSelected;

		public ActorViewModel? SelectedActor { get; private set; }
		public ObservableCollection<ActorTableActor> Actors { get; set; } = new ObservableCollection<ActorTableActor>();

		public override Task Start()
		{
			Task.Run(this.Watch);

			return base.Start();
		}

		public void SelectActor(ActorViewModel? actor)
		{
			this.SelectedActor = actor;

			/*using IMarshaler<int> territoryMem = MemoryService.GetMarshaler(Offsets.Main.TerritoryAddress, Offsets.Main.Territory);

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
			}*/

			ActorSelected?.Invoke(actor);
		}

		private async Task Watch()
		{
			try
			{
				await Task.Delay(500);

				IntPtr lastTargetAddress = IntPtr.Zero;

				while (this.IsAlive)
				{
					await Task.Delay(50);

					while (ActorRefreshService.Instance.IsRefreshing || GposeService.Instance.IsChangingState)
						await Task.Delay(250);

					IntPtr newTargetAddress;
					if (GposeService.Instance.IsGpose)
					{
						newTargetAddress = MemoryService.ReadPtr(AddressService.GPoseTargetManager);
					}
					else
					{
						newTargetAddress = MemoryService.ReadPtr(AddressService.TargetManager);

						List<IntPtr> actorPointers = new List<IntPtr>();
						for (int i = 0; i < 424; i++)
						{
							IntPtr ptr = MemoryService.ReadPtr(AddressService.ActorTable + (i * 8));

							if (ptr == IntPtr.Zero)
								continue;

							actorPointers.Add(ptr);
						}

						this.UpdateActorList(actorPointers);
					}

					if (newTargetAddress != lastTargetAddress)
					{
						lastTargetAddress = newTargetAddress;

						try
						{
							if (newTargetAddress != IntPtr.Zero)
							{
								ActorViewModel vm = new ActorViewModel(newTargetAddress);
								this.SelectActor(vm);
							}
						}
						catch (Exception ex)
						{
							Log.Write(Severity.Warning, new Exception("Failed to select current target", ex));
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private void UpdateActorList(List<IntPtr> pointers)
		{
			if (App.Current == null)
				return;

			App.Current.Dispatcher.Invoke(() =>
			{
				// Remove missing actors, and remove existing pointers
				for (int i = this.Actors.Count - 1; i >= 0; i--)
				{
					if (pointers.Contains(this.Actors[i].Pointer))
					{
						pointers.Remove(this.Actors[i].Pointer);
					}
					else
					{
						this.Actors.RemoveAt(i);
					}
				}

				// now add new actors
				foreach (IntPtr pointer in pointers)
				{
					Actor actor = MemoryService.Read<Actor>(pointer);

					if (actor.ObjectKind != ActorTypes.Player
					 && actor.ObjectKind != ActorTypes.BattleNpc
					 && actor.ObjectKind != ActorTypes.EventNpc
					 && actor.ObjectKind != ActorTypes.Companion
					 && actor.ObjectKind != ActorTypes.Retainer)
						continue;

					if (string.IsNullOrEmpty(actor.Name))
						continue;

					this.Actors.Add(new ActorTableActor(actor, pointer));
				}
			});
		}

		public class ActorTableActor
		{
			public readonly IntPtr Pointer;

			private Actor actor;

			public ActorTableActor(Actor actor, IntPtr pointer)
			{
				this.actor = actor;
				this.Pointer = pointer;
			}

			public string Name => this.actor.Name;
			public ActorTypes Kind => this.actor.ObjectKind;
			public IconChar Icon => this.actor.ObjectKind.GetIcon();
		}
	}
}
