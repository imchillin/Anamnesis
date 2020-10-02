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
		public ObservableCollection<ActorTableActor> PinnedActors { get; set; } = new ObservableCollection<ActorTableActor>();

		public static void AddActor(ActorTableActor actor)
		{
			foreach (ActorTableActor otherActor in Instance.PinnedActors)
			{
				if (actor.Pointer == otherActor.Pointer)
				{
					return;
				}
			}

			Instance.PinnedActors.Add(actor);
		}

		public static void RemoveActor(ActorTableActor actor)
		{
			Instance.PinnedActors.Remove(actor);
		}

		public static List<ActorTableActor> GetActors()
		{
			List<ActorTableActor> actorPointers = new List<ActorTableActor>();

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

			List<ActorTableActor> results = new List<ActorTableActor>();
			for (int i = 0; i < count; i++)
			{
				IntPtr ptr = MemoryService.ReadPtr(startAddress + (i * 8));

				if (ptr == IntPtr.Zero)
					continue;

				Actor actor = MemoryService.Read<Actor>(ptr);

				if (actor.ObjectKind != ActorTypes.Player
						&& actor.ObjectKind != ActorTypes.BattleNpc
						&& actor.ObjectKind != ActorTypes.EventNpc
						&& actor.ObjectKind != ActorTypes.Companion
						&& actor.ObjectKind != ActorTypes.Retainer)
					continue;

				if (string.IsNullOrEmpty(actor.Name))
					continue;

				results.Add(new ActorTableActor(actor, ptr));
			}

			return results;
		}

		public override async Task Start()
		{
			await base.Start();

			List<ActorTableActor> actors = TargetService.GetActors();

			if (actors.Count <= 0)
				return;

			this.PinnedActors.Add(actors[0]);
			this.SelectActor(actors[0]);
		}

		public void ClearSelection()
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				this.SelectedActor = null;
				this.PinnedActors.Clear();
			});
		}

		public void Retarget()
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				this.SelectedActor = null;
			});

			if (this.PinnedActors.Count > 0)
			{
				foreach (ActorTableActor actor in this.PinnedActors)
				{
					actor.Clear();
				}

				this.SelectActor(this.PinnedActors[0]);
			}
		}

		public void SelectActor(ActorTableActor actor)
		{
			this.SelectActor(actor.GetViewModel());

			foreach (ActorTableActor ac in this.PinnedActors)
			{
				ac.SelectionChanged();
			}
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

		/*private async Task Watch()
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

					if (this.SelectedActor == null && this.AllActors.Count > 0)
					{
						this.ReadActorLists();

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
		}*/

		[AddINotifyPropertyChangedInterface]
		public class ActorTableActor : INotifyPropertyChanged
		{
			private Actor actor;
			private ActorViewModel? viewModel;

			public ActorTableActor(Actor actor, IntPtr pointer)
			{
				this.actor = actor;
				this.Pointer = pointer;
				this.IsValid = true;

				this.Initials = string.Empty;
				string[] names = actor.Name.Split(' ');
				foreach (string name in names)
				{
					this.Initials += name[0] + ".";
				}

				this.Initials = this.Initials.Trim('.');
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

			public IntPtr? Pointer { get; private set; }
			public string Name => this.actor.Name;
			public ActorTypes Kind => this.actor.ObjectKind;
			public IconChar Icon => this.actor.ObjectKind.GetIcon();
			public string Initials { get; private set; }
			public bool IsValid { get; private set; }

			public void SelectionChanged()
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
			}

			/// <summary>
			/// Compares actor identity, does not compare pointers.
			/// </summary>
			public bool Is(ActorTableActor other)
			{
				// TODO: Handle cases where multiple actors share a name, but are different actors
				// perhaps compare modelType and customize values?
				return this.Name == other.Name;
			}

			public void Clear()
			{
				this.viewModel = null;
				this.Pointer = null;
			}

			public ActorViewModel? GetViewModel()
			{
				if (this.Pointer == null)
					this.Retarget();

				if (this.viewModel == null || this.Pointer != this.viewModel.Pointer || this.Name != this.viewModel.Name)
					this.Retarget();

				return this.viewModel;
			}

			public override bool Equals(object? obj)
			{
				return obj is ActorTableActor actor &&
					   EqualityComparer<IntPtr?>.Default.Equals(this.Pointer, actor.Pointer) &&
					   this.Name == actor.Name;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(this.Pointer, this.Name);
			}

			private void Retarget()
			{
				this.viewModel = null;

				List<ActorTableActor> actors = TargetService.GetActors();

				foreach (ActorTableActor actor in actors)
				{
					if (actor.Name == this.Name && actor.Pointer != null)
					{
						ActorViewModel vm = new ActorViewModel((IntPtr)actor.Pointer);

						// Handle case where multiple actor table entries point ot the same actor, but
						// its not the actor we actually want.
						if (vm.Name != this.Name)
							continue;

						this.Pointer = actor.Pointer;
						this.actor = actor.actor;
						this.viewModel = vm;
					}
				}

				this.IsValid = this.viewModel != null;
			}
		}
	}
}
