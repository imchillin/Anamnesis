// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Media.Animation;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.WpfStyles;
	using FontAwesome.Sharp;
	using PropertyChanged;
	using SimpleLog;

	public delegate void SelectionEvent(ActorViewModel? actor);

	[AddINotifyPropertyChangedInterface]
	public class TargetService : ServiceBase<TargetService>
	{
		public static event SelectionEvent? ActorSelected;

		public ActorViewModel? SelectedActor { get; private set; }
		public ObservableCollection<ActorTableActor> Actors { get; set; } = new ObservableCollection<ActorTableActor>();
		public ObservableCollection<ActorTableActor> AllActors { get; set; } = new ObservableCollection<ActorTableActor>();

		public static void AddActor(ActorTableActor actor)
		{
			foreach (ActorTableActor otherActor in Instance.Actors)
			{
				if (actor.Pointer == otherActor.Pointer)
				{
					return;
				}
			}

			Instance.Actors.Add(actor);
		}

		public static void RemoveActor(ActorTableActor actor)
		{
			Instance.Actors.Remove(actor);
		}

		public override Task Start()
		{
			Task.Run(this.Watch);

			return base.Start();
		}

		public void ClearSelection()
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				this.SelectedActor = null;
				this.AllActors.Clear();
				this.Actors.Clear();
			});
		}

		public void SelectActor(ActorTableActor actor)
		{
			ActorViewModel vm = new ActorViewModel(actor.Pointer);
			this.SelectActor(vm);

			foreach (ActorTableActor ac in this.Actors)
			{
				ac.SelectionChanged();
			}
		}

		public void SelectActor(ActorViewModel? actor)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			this.SelectedActor = actor;

			sw.Stop();
			Log.Write("took " + sw.ElapsedMilliseconds + " ms to change selected actor");

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

					List<IntPtr> actorPointers = new List<IntPtr>();

					int count = 0;
					IntPtr startAddress;

					if (GposeService.Instance.IsGpose)
					{
						count = MemoryService.Read<int>(AddressService.GPoseActorTable);
						startAddress = AddressService.GPoseActorTable + 8;
						////ingameTargetAddress = MemoryService.ReadPtr(AddressService.GPoseTargetManager);
					}
					else
					{
						// why 424?
						count = 424;
						startAddress = AddressService.ActorTable;
						////ingameTargetAddress = MemoryService.ReadPtr(AddressService.TargetManager);
					}

					for (int i = 0; i < count; i++)
					{
						IntPtr ptr = MemoryService.ReadPtr(startAddress + (i * 8));

						if (ptr == IntPtr.Zero)
							continue;

						actorPointers.Add(ptr);
					}

					this.UpdateActorList(actorPointers);

					if (this.SelectedActor == null && this.AllActors.Count > 0)
					{
						App.Current.Dispatcher.Invoke(() =>
						{
							AddActor(this.AllActors[0]);
							this.SelectActor(this.Actors[0]);
						});
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
				for (int i = this.AllActors.Count - 1; i >= 0; i--)
				{
					if (pointers.Contains(this.AllActors[i].Pointer))
					{
						pointers.Remove(this.AllActors[i].Pointer);
					}
					else
					{
						this.AllActors.RemoveAt(i);
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

					this.AllActors.Add(new ActorTableActor(actor, pointer));
				}
			});
		}

		[AddINotifyPropertyChangedInterface]
		public class ActorTableActor : INotifyPropertyChanged
		{
			public readonly IntPtr Pointer;

			private Actor actor;

			public ActorTableActor(Actor actor, IntPtr pointer)
			{
				this.actor = actor;
				this.Pointer = pointer;

				Regex initialsReg = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
				this.Initials = initialsReg.Replace(actor.Name, "$1.").Trim('.');
			}

			public event PropertyChangedEventHandler? PropertyChanged;

			public bool IsSelected
			{
				get
				{
					return TargetService.Instance.SelectedActor?.Pointer == this.Pointer;
				}

				set
				{
					if (value)
					{
						TargetService.Instance.SelectActor(this);
					}
				}
			}

			public string Name => this.actor.Name;
			public ActorTypes Kind => this.actor.ObjectKind;
			public IconChar Icon => this.actor.ObjectKind.GetIcon();
			public string Initials { get; private set; }

			public void SelectionChanged()
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
			}

			public override bool Equals(object? obj)
			{
				return obj is ActorTableActor actor &&
					   this.Pointer.Equals(actor.Pointer);
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(this.Pointer);
			}
		}
	}
}
