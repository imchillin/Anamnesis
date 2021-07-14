// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.Core.Memory;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.Styles;
	using FontAwesome.Sharp;
	using PropertyChanged;
	using XivToolsWpf;

	public delegate void SelectionEvent(ActorViewModel? actor);
	public delegate void PinnedEvent(TargetService.ActorTableActor actor);

	[AddINotifyPropertyChangedInterface]
	public class TargetService : ServiceBase<TargetService>
	{
		public static event SelectionEvent? ActorSelected;
		public static event PinnedEvent? ActorPinned;

		public ActorViewModel? SelectedActor { get; private set; }
		public ObservableCollection<ActorTableActor> PinnedActors { get; set; } = new ObservableCollection<ActorTableActor>();

		public static async Task PinActor(ActorTableActor actor)
		{
			foreach (ActorTableActor otherActor in Instance.PinnedActors)
			{
				if (actor.Pointer == otherActor.Pointer)
				{
					return;
				}
			}

			// Mannequins and housing NPC's get actor type changed, but squadron members and lawn retainers do not.
			if (actor.Kind == ActorTypes.EventNpc && actor.Model.DataId != 1011832)
			{
				bool? result = await GenericDialog.Show(LocalizationService.GetStringFormatted("Target_ConvertHousingNpcToPlayerMessage", actor.DisplayName), LocalizationService.GetString("Target_ConvertToPlayerTitle"), MessageBoxButton.YesNo);
				if (result == true)
				{
					ActorViewModel? vm = actor.GetViewModel();
					if (vm == null)
						return;

					await vm.ConvertToPlayer();
					actor.Model = (Actor)vm.Model!;
				}
			}

			// Carbuncles get model type set to player (but not actor type!)
			if (actor.Kind == ActorTypes.BattleNpc && (actor.ModelType == 1 || actor.ModelType == 409 || actor.ModelType == 410 || actor.ModelType == 412))
			{
				bool? result = await GenericDialog.Show(LocalizationService.GetStringFormatted("Target_ConvertCarbuncleToPlayerMessage", actor.DisplayName), LocalizationService.GetString("Target_ConvertToPlayerTitle"), MessageBoxButton.YesNo);
				if (result == true)
				{
					ActorViewModel? vm = actor.GetViewModel();
					if (vm == null)
						return;

					await vm.ConvertToPlayer();
					actor.Model = (Actor)vm.Model!;
				}
			}

			await Dispatch.MainThread();
			Instance.PinnedActors.Add(actor);
			Instance.SelectActor(actor);
			ActorPinned?.Invoke(actor);
		}

		public static void UnpinActor(ActorTableActor actor)
		{
			Instance.PinnedActors.Remove(actor);

			if (actor.GetViewModel() == Instance.SelectedActor)
			{
				if (Instance.PinnedActors.Count > 0)
				{
					Instance.SelectActor(Instance.PinnedActors[0]);
				}
				else
				{
					Instance.SelectActorViewModel(null);
				}
			}

			actor.Clear();
		}

		public static List<ActorTableActor> GetActors()
		{
			List<ActorTableActor> actorPointers = new List<ActorTableActor>();

			int count = 0;
			IntPtr startAddress;

			if (GposeService.Instance.GetIsGPose())
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

			await PinActor(actors[0]);
		}

		public void ClearSelection()
		{
			if (this.SelectedActor == null)
				return;

			if (App.Current == null)
				return;

			App.Current.Dispatcher.Invoke(() =>
			{
				this.SelectedActor = null;

				foreach (ActorTableActor actor in this.PinnedActors)
				{
					actor.SelectionChanged();
				}
			});
		}

		public void EnsureSelection()
		{
			if (App.Current == null)
				return;

			if (this.SelectedActor != null)
				return;

			if (this.PinnedActors == null || this.PinnedActors.Count <= 0)
				return;

			this.SelectActor(this.PinnedActors[0]);
		}

		public void ClearPins()
		{
			if (this.SelectedActor == null)
				return;

			if (App.Current == null)
				return;

			App.Current.Dispatcher.Invoke(() =>
			{
				this.SelectedActor = null;
				this.PinnedActors.Clear();
			});
		}

		public async Task Retarget()
		{
			await Dispatch.MainThread();
			this.SelectedActor = null;

			if (this.PinnedActors.Count > 0)
			{
				this.SelectActor(this.PinnedActors[0]);
			}
		}

		public void SelectActor(ActorTableActor actor)
		{
			this.SelectActorViewModel(actor.GetViewModel());

			foreach (ActorTableActor ac in this.PinnedActors)
			{
				ac.SelectionChanged();
			}
		}

		public void SelectActorViewModel(ActorViewModel? actor)
		{
			this.SelectedActor = actor;
			ActorSelected?.Invoke(actor);
		}

		[AddINotifyPropertyChangedInterface]
		public class ActorTableActor : INotifyPropertyChanged
		{
			private ActorViewModel? viewModel;

			private string name;

			public ActorTableActor(Actor actor, IntPtr pointer)
			{
				this.Model = actor;
				this.Pointer = pointer;
				this.IsValid = true;
				this.Initials = string.Empty;
				this.name = actor.Name;

				this.UpdateInitials(this.DisplayName);
			}

			public event PropertyChangedEventHandler? PropertyChanged;

			public Actor Model { get; set; }
			public string Id => this.Model.Id;
			public IntPtr? Pointer { get; private set; }
			public ActorTypes Kind => this.Model.ObjectKind;
			public IconChar Icon => this.Model.ObjectKind.GetIcon();
			public int ModelType => this.Model.ModelType;
			public string Initials { get; private set; }
			public bool IsValid { get; private set; }
			public bool IsPinned => TargetService.Instance.PinnedActors.Contains(this);

			public string DisplayName => this.viewModel == null ? this.name : this.viewModel.DisplayName;

			public float DistanceFromPlayer
			{
				get
				{
					return new System.Numerics.Vector2(this.Model.DistanceFromPlayerX, this.Model.DistanceFromPlayerY).Length();
				}
			}

			public bool IsSelected
			{
				get
				{
					if (this.Pointer == null)
						return false;

					return TargetService.Instance.SelectedActor?.Pointer == this.Pointer;
				}

				set
				{
					if (!GameService.Instance.IsSignedIn)
						return;

					if (value)
					{
						TargetService.Instance.SelectActor(this);
					}
				}
			}

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
				return this.Id == other.Id;
			}

			public void Clear()
			{
				this.Pointer = null;

				if (this.viewModel != null)
					this.viewModel.Dispose();

				this.viewModel = null;
			}

			public ActorViewModel? GetViewModel()
			{
				this.Retarget();
				return this.viewModel;
			}

			public override bool Equals(object? obj)
			{
				return obj is ActorTableActor actor &&
					   EqualityComparer<IntPtr?>.Default.Equals(this.Pointer, actor.Pointer) &&
					   this.name == actor.name;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(this.Pointer, this.name);
			}

			private void Retarget()
			{
				lock (this)
				{
					if (this.viewModel != null)
						this.viewModel.PropertyChanged -= this.OnViewModelPropertyChanged;

					List<ActorTableActor> actors = TargetService.GetActors();
					bool found = false;
					foreach (ActorTableActor actor in actors)
					{
						if (actor.Id == this.Id && actor.Pointer != null)
						{
							// Handle case where multiple actor table entries point ot the same actor, but
							// its not the actor we actually want.
							////if (actor.Id != this.Id)
							////	continue;

							if (this.viewModel != null)
							{
								this.viewModel.Pointer = actor.Pointer;
							}
							else
							{
								this.viewModel = new ActorViewModel((IntPtr)actor.Pointer);
							}

							this.Pointer = actor.Pointer;
							this.Model = actor.Model;
							this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;
							found = true;
							break;
						}
					}

					if (!found)
					{
						this.viewModel?.Dispose();
						this.viewModel = null;
					}
					else if (this.viewModel != null)
					{
						this.name = this.viewModel.Name;
						this.viewModel.OnRetargeted();
					}

					Log.Information($"Retargeting actor: {this.Initials}. Success: {found}. Checked {actors.Count} actors.");
					this.IsValid = found;
				}
			}

			private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (this.viewModel != null && e.PropertyName == nameof(ActorViewModel.DisplayName))
				{
					this.UpdateInitials(this.viewModel.DisplayName);
				}
			}

			private void UpdateInitials(string name)
			{
				if (string.IsNullOrWhiteSpace(name))
					return;

				try
				{
					if (name.Length <= 4)
					{
						this.Initials = name;
					}
					else
					{
						this.Initials = string.Empty;

						string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
						foreach (string part in parts)
						{
							this.Initials += part[0] + ".";
						}

						this.Initials = this.Initials.Trim('.');
					}
				}
				catch (Exception)
				{
					this.Initials = name[0] + "?";
				}
			}
		}
	}
}