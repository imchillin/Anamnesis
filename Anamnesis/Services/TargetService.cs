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
	public delegate void PinnedEvent(TargetService.PinnedActor actor);

	[AddINotifyPropertyChangedInterface]
	public class TargetService : ServiceBase<TargetService>
	{
		public static event SelectionEvent? ActorSelected;
		public static event PinnedEvent? ActorPinned;

		public ActorViewModel? SelectedActor { get; private set; }
		public ObservableCollection<PinnedActor> PinnedActors { get; set; } = new ObservableCollection<PinnedActor>();

		public static async Task PinActor(ActorBasicViewModel basicActor)
		{
			if (basicActor.Pointer == null)
				return;

			foreach (PinnedActor otherActor in Instance.PinnedActors)
			{
				if (basicActor.Pointer == otherActor.Pointer)
				{
					return;
				}
			}

			ActorViewModel actor = new ActorViewModel((IntPtr)basicActor.Pointer);

			// Mannequins and housing NPC's get actor type changed, but squadron members and lawn retainers do not.
			if (actor.ObjectKind == ActorTypes.EventNpc && actor.DataId != 1011832)
			{
				bool? result = await GenericDialog.Show(LocalizationService.GetStringFormatted("Target_ConvertHousingNpcToPlayerMessage", actor.Name), LocalizationService.GetString("Target_ConvertToPlayerTitle"), MessageBoxButton.YesNo);
				if (result == true)
				{
					await actor.ConvertToPlayer();
				}
			}

			// Carbuncles get model type set to player (but not actor type!)
			if (actor.ObjectKind == ActorTypes.BattleNpc && (actor.ModelType == 1 || actor.ModelType == 409 || actor.ModelType == 410 || actor.ModelType == 412))
			{
				bool? result = await GenericDialog.Show(LocalizationService.GetStringFormatted("Target_ConvertCarbuncleToPlayerMessage", actor.DisplayName), LocalizationService.GetString("Target_ConvertToPlayerTitle"), MessageBoxButton.YesNo);
				if (result == true)
				{
					await actor.ConvertToPlayer();
				}
			}

			PinnedActor pined = new PinnedActor(actor);

			await Dispatch.MainThread();
			Instance.PinnedActors.Add(pined);
			Instance.SelectActor(pined);
			ActorPinned?.Invoke(pined);
		}

		public static void UnpinActor(PinnedActor actor)
		{
			Instance.PinnedActors.Remove(actor);

			if (actor.ViewModel == Instance.SelectedActor)
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
		}

		public static bool IsPinned(ActorBasicViewModel actor)
		{
			foreach (PinnedActor pinned in Instance.PinnedActors)
			{
				if (pinned.Id == actor.Id)
				{
					return true;
				}
			}

			return false;
		}

		public static List<ActorBasicViewModel> GetAllActors()
		{
			int count = 0;
			IntPtr startAddress;

			if (GposeService.Instance.GetIsGPose())
			{
				count = MemoryService.Read<int>(AddressService.GPoseActorTable);
				startAddress = AddressService.GPoseActorTable + 8;
			}
			else
			{
				// why 424?
				count = 424;
				startAddress = AddressService.ActorTable;
			}

			List<ActorBasicViewModel> results = new();
			for (int i = 0; i < count; i++)
			{
				IntPtr ptr = MemoryService.ReadPtr(startAddress + (i * 8));

				if (ptr == IntPtr.Zero)
					continue;

				results.Add(new ActorBasicViewModel(ptr));
			}

			return results;
		}

		public override async Task Start()
		{
			await base.Start();

			List<ActorBasicViewModel> allaCtors = GetAllActors();

			if (allaCtors.Count > 0)
			{
				await PinActor(allaCtors[0]);
			}

			_ = Task.Run(this.TickPinnedActors);
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

				foreach (PinnedActor actor in this.PinnedActors)
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

		public void SelectActor(PinnedActor actor)
		{
			this.SelectActorViewModel(actor.GetViewModel());

			foreach (PinnedActor ac in this.PinnedActors)
			{
				ac.SelectionChanged();
			}
		}

		public void SelectActorViewModel(ActorViewModel? actor)
		{
			this.SelectedActor = actor;
			ActorSelected?.Invoke(actor);
		}

		private async Task TickPinnedActors()
		{
			while (this.IsAlive)
			{
				await Task.Delay(33);

				foreach (PinnedActor actor in this.PinnedActors)
				{
					actor.Tick();
				}
			}
		}

		[AddINotifyPropertyChangedInterface]
		public class PinnedActor : INotifyPropertyChanged
		{
			public PinnedActor(ActorViewModel actorVm)
			{
				this.Id = actorVm.Id;
				this.ViewModel = actorVm;
				this.Retarget();
			}

			public event PropertyChangedEventHandler? PropertyChanged;

			public ActorViewModel? ViewModel { get; private set; }

			public string? Name { get; private set; }
			public string Id { get; private set; }
			public IntPtr? Pointer { get; private set; }
			public ActorTypes Kind { get; private set; }
			public IconChar Icon => this.Kind.GetIcon();
			public int ModelType { get; private set; }
			public string? Initials { get; private set; }
			public bool IsValid { get; private set; }
			public bool IsPinned => TargetService.Instance.PinnedActors.Contains(this);

			public string? DisplayName => this.ViewModel == null ? this.Name : this.ViewModel.DisplayName;
			public bool IsRetargeting { get; private set; } = false;

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

			public override string? ToString()
			{
				if (this.ViewModel == null)
					return base.ToString();

				return this.ViewModel.DisplayName;
			}

			public void Dispose()
			{
				this.ViewModel?.Dispose();
			}

			public void SelectionChanged()
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
			}

			public ActorViewModel? GetViewModel()
			{
				this.Retarget();
				return this.ViewModel;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(this.Pointer, this.Name);
			}

			public void Tick()
			{
				lock (this)
				{
					if (this.IsRetargeting)
						return;

					if (this.ViewModel == null)
						return;

					try
					{
						this.ViewModel.ReadChanges();
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Failed to tick selected actor");
					}
				}
			}

			private void Retarget()
			{
				lock (this)
				{
					this.IsRetargeting = true;

					if (this.ViewModel != null)
						this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;

					foreach (ActorBasicViewModel actor in TargetService.GetAllActors())
					{
						if (actor.Id != this.Id || actor.Pointer == null)
							continue;

						if (this.ViewModel != null)
						{
							this.ViewModel.Pointer = actor.Pointer;
							this.ViewModel.ReadChanges();
						}
						else
						{
							this.ViewModel = new ActorViewModel((IntPtr)actor.Pointer);
						}

						this.Name = this.ViewModel.Name;
						this.ViewModel.OnRetargeted();
						this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
						this.Pointer = this.ViewModel.Pointer;
						this.Kind = this.ViewModel.ObjectKind;
						this.ModelType = this.ViewModel.ModelType;

						this.UpdateInitials(this.DisplayName);

						this.IsValid = true;
						Log.Information($"Retargeted actor: {this.Initials}");

						this.IsRetargeting = false;

						return;
					}

					this.ViewModel?.Dispose();
					Log.Warning($"Lost actor: {this.Initials}");

					this.IsValid = false;
					this.IsRetargeting = false;
				}
			}

			private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (this.ViewModel != null && e.PropertyName == nameof(ActorViewModel.DisplayName))
				{
					this.UpdateInitials(this.ViewModel.DisplayName);
				}
			}

			private void UpdateInitials(string? name)
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