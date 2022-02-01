// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.Styles;
	using FontAwesome.Sharp;
	using PropertyChanged;
	using XivToolsWpf;

	public delegate void SelectionEvent(ActorMemory? actor);
	public delegate void PinnedEvent(TargetService.PinnedActor actor);

	[AddINotifyPropertyChangedInterface]
	public class TargetService : ServiceBase<TargetService>
	{
		public static readonly int OverworldPlayerTargetOffset = 0x80;
		public static readonly int GPosePlayerTargetOffset = 0x98;

		public static event SelectionEvent? ActorSelected;
		public static event PinnedEvent? ActorPinned;
		public static event PinnedEvent? ActorUnPinned;

		public ActorBasicMemory PlayerTarget { get; private set; } = new();
		public bool IsPlayerTargetPinnable => this.PlayerTarget.Address != IntPtr.Zero && this.PlayerTarget.ObjectKind.IsSupportedType();
		public ActorMemory? SelectedActor { get; private set; }
		public ObservableCollection<PinnedActor> PinnedActors { get; set; } = new ObservableCollection<PinnedActor>();

		public static async Task PinActor(ActorBasicMemory basicActor)
		{
			if (basicActor.Address == IntPtr.Zero)
				return;

			if (!basicActor.ObjectKind.IsSupportedType())
			{
				Log.Warning($"You cannot pin actor of type: {basicActor.ObjectKind}");
				return;
			}

			try
			{
				foreach (PinnedActor otherActor in Instance.PinnedActors)
				{
					if (basicActor.Address == otherActor.Pointer)
					{
						Log.Information($"Actor already pinned: {otherActor}");
						Instance.SelectActor(otherActor);
						return;
					}
				}

				ActorMemory memory = new();
				memory.SetAddress(basicActor.Address);
				PinnedActor pined = new PinnedActor(memory);

				Log.Information($"Pinning actor: {pined}");

				await Dispatch.MainThread();
				Instance.PinnedActors.Add(pined);
				Instance.SelectActor(pined);
				ActorPinned?.Invoke(pined);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to pin actor");
			}
		}

		public static async Task PinPlayerTargetedActor()
		{
			Instance.UpdatePlayerTarget();
			await PinActor(Instance.PlayerTarget);
		}

		public static void UnpinActor(PinnedActor actor)
		{
			Instance.PinnedActors.Remove(actor);

			if (actor.Memory == Instance.SelectedActor)
			{
				if (Instance.PinnedActors.Count > 0)
				{
					Instance.SelectActor(Instance.PinnedActors[0]);
				}
				else
				{
					Instance.SelectActor((ActorMemory?)null);
				}
			}

			ActorUnPinned?.Invoke(actor);
		}

		public static PinnedActor? GetPinned(ActorBasicMemory actor)
		{
			foreach (PinnedActor pinned in TargetService.Instance.PinnedActors)
			{
				if (pinned.Memory == null)
					continue;

				if (pinned.Memory.Id == actor.Id)
				{
					return pinned;
				}
			}

			return null;
		}

		public static bool IsPinned(ActorBasicMemory actor)
		{
			return GetPinned(actor) != null;
		}

		public static void SetPlayerTarget(PinnedActor actor)
		{
			if (actor.IsValid)
			{
				IntPtr? ptr = actor.Pointer;
				if (ptr != null && ptr != IntPtr.Zero)
				{
					if (ActorService.Instance.IsActorInTable((IntPtr)ptr))
					{
						if (GposeService.Instance.IsGpose)
						{
							MemoryService.Write<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, GPosePlayerTargetOffset), (IntPtr)ptr, "Update player target");
						}
						else
						{
							MemoryService.Write<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, OverworldPlayerTargetOffset), (IntPtr)ptr, "Update player target");
						}
					}
				}
			}
		}

		public void UpdatePlayerTarget()
		{
			IntPtr currentPlayerTargetPtr = IntPtr.Zero;

			try
			{
				if (GposeService.Instance.IsGpose)
				{
					currentPlayerTargetPtr = MemoryService.Read<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, GPosePlayerTargetOffset));
				}
				else
				{
					currentPlayerTargetPtr = MemoryService.Read<IntPtr>(IntPtr.Add(AddressService.PlayerTargetSystem, OverworldPlayerTargetOffset));
				}
			}
			catch
			{
				// If the memory read fails the target will be 0x0
			}

			try
			{
				if (currentPlayerTargetPtr != this.PlayerTarget.Address)
				{
					if (currentPlayerTargetPtr == IntPtr.Zero)
					{
						this.PlayerTarget.Dispose();
					}
					else
					{
						this.PlayerTarget.SetAddress(currentPlayerTargetPtr);
					}

					this.RaisePropertyChanged(nameof(TargetService.PlayerTarget));
					this.RaisePropertyChanged(nameof(TargetService.IsPlayerTargetPinnable));
				}
			}
			catch
			{
				// This section can only fail when FFXIV isn't running (fail to set address) so it should be safe to ignore
			}
		}

		public override async Task Start()
		{
			await base.Start();

			if (GameService.GetIsSignedIn())
			{
				try
				{
					bool isGpose = GposeService.GetIsGPose();

					List<ActorBasicMemory> allActors = ActorService.Instance.GetAllActors();

					// We want the first non-hidden actor with a name in the same mode as the game
					foreach (ActorBasicMemory actor in allActors)
					{
						if (actor.IsHidden)
							continue;

						if (string.IsNullOrEmpty(actor.Name))
							continue;

						if (actor.IsGPoseActor != isGpose)
							continue;

						await PinActor(actor);
						break;
					}
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed to pin default actor");
				}
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
			this.SelectActor(actor.GetMemory());

			foreach (PinnedActor ac in this.PinnedActors)
			{
				ac.SelectionChanged();
			}
		}

		public void SelectActor(ActorMemory? actor)
		{
			this.SelectedActor = actor;
			ActorSelected?.Invoke(actor);
		}

		private async Task TickPinnedActors()
		{
			while (this.IsAlive)
			{
				await Task.Delay(33);

				for (int i = this.PinnedActors.Count - 1; i >= 0; i--)
				{
					this.PinnedActors[i].Tick();
				}

				this.UpdatePlayerTarget();
			}
		}

		[AddINotifyPropertyChangedInterface]
		public class PinnedActor : INotifyPropertyChanged
		{
			public PinnedActor(ActorMemory memory)
			{
				this.Id = memory.Id;
				this.IdNoAddress = memory.IdNoAddress;
				this.Memory = memory;
				this.UpdateActorInfo();
			}

			public event PropertyChangedEventHandler? PropertyChanged;

			public ActorMemory? Memory { get; private set; }
			////public ActorViewModel? ViewModel { get; private set; }

			public string? Name { get; private set; }
			public string Id { get; private set; }
			public string IdNoAddress { get; private set; }
			public IntPtr? Pointer { get; private set; }
			public ActorTypes Kind { get; private set; }
			public IconChar Icon => this.Kind.GetIcon();
			public int ModelType { get; private set; }
			public string? Initials { get; private set; }
			public bool IsValid { get; private set; }
			public bool IsGPoseActor { get; private set; }
			public bool IsHidden { get; private set; }
			public bool IsPinned => TargetService.Instance.PinnedActors.Contains(this);

			public string? DisplayName => this.Memory == null ? this.Name : this.Memory.DisplayName;
			public bool IsRetargeting { get; private set; } = false;

			public bool IsSelected
			{
				get
				{
					if (this.Pointer == null)
						return false;

					if (this.Memory != null && TargetService.Instance.SelectedActor == this.Memory)
						return true;

					return TargetService.Instance.SelectedActor?.Address == this.Pointer;
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
				if (this.Memory == null)
					return base.ToString();

				return this.Memory.ToString();
			}

			public void Dispose()
			{
			}

			public void SelectionChanged()
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
			}

			public ActorMemory? GetMemory()
			{
				this.Tick();
				return this.Memory;
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

					if (!this.IsValid)
					{
						this.Retarget();
						return;
					}

					if (this.Memory == null || this.Memory.Address == IntPtr.Zero)
						return;

					if (!ActorService.Instance.IsActorInTable(this.Memory.Address))
					{
						Log.Information($"Actor: {this} was not in actor table");
						this.Retarget();
						return;
					}

					if (this.IsGPoseActor && GposeService.Instance.IsOverworld)
					{
						Log.Information($"Actor: {this} was a gpose actor and we are now in the overworld");
						this.Retarget();
						return;
					}

					if (this.Memory.IsHidden && !this.IsHidden && !this.IsGPoseActor && !this.Memory.IsRefreshing && GposeService.Instance.IsGpose)
					{
						Log.Information($"Actor: {this} was hidden entering the gpose boundary");
						this.Retarget();
						return;
					}

					try
					{
						if (GposeService.Instance.IsChangingState)
							return;

						this.Memory.Tick();
					}
					catch (Exception ex)
					{
						Log.Warning(ex, "Failed to tick actor");
						this.SetInvalid();
					}
				}
			}

			private void SetInvalid()
			{
				if (this.Memory != null)
				{
					if (this.IsSelected)
						TargetService.Instance.ClearSelection();

					this.Memory.Dispose();
					this.Memory = null;
				}

				this.IsValid = false;
			}

			private void Retarget()
			{
				lock (this)
				{
					this.IsRetargeting = true;

					if (this.Memory != null)
						this.Memory.PropertyChanged -= this.OnViewModelPropertyChanged;

					ActorBasicMemory? newBasic = null;
					bool isGPose = GposeService.GetIsGPose();

					List<ActorBasicMemory> allActors = ActorService.Instance.GetAllActors();

					// Search for an exact match first
					foreach (ActorBasicMemory actor in allActors)
					{
						if (actor.Id != this.Id || actor.Address == IntPtr.Zero)
							continue;

						// Don't consider hidden actors for retargeting
						if (actor.IsHidden)
							continue;

						newBasic = actor;
						break;
					}

					// fall back to ignoring addresses
					if (newBasic == null)
					{
						foreach (ActorBasicMemory actor in allActors)
						{
							if (actor.IdNoAddress != this.IdNoAddress || actor.Address == IntPtr.Zero)
								continue;

							// Don't consider hidden actors for retargeting
							if(actor.IsHidden)
								continue;

							// Don't consider overworld actors while we are in gpose
							////if (isGPose && actor.IsOverworldActor)
							////	continue;

							// Is this actor memory already pinned to a differnet pin?
							PinnedActor? pinned = TargetService.GetPinned(actor);
							if (pinned != this && pinned != null)
								continue;

							newBasic = actor;
							break;
						}
					}

					if (newBasic != null)
					{
						if (this.Memory != null)
						{
							////this.Memory.Address = newBasic.Address;
							this.Memory.SetAddress(newBasic.Address);

							try
							{
								this.Memory.Tick();
							}
							catch (Exception ex)
							{
								Log.Warning(ex, "Failed to tick actor");
								this.SetInvalid();
								return;
							}
						}
						else
						{
							this.Memory = new ActorMemory();
							this.Memory.SetAddress(newBasic.Address);
						}

						IntPtr? oldPointer = this.Pointer;

						this.UpdateActorInfo();

						// dont log every time we just select an actor.
						if (oldPointer != null && oldPointer != this.Pointer)
							Log.Information($"Retargeted actor: {this} from {oldPointer} to {this.Pointer}");

						this.IsRetargeting = false;

						return;
					}

					if (this.Memory != null)
					{
						Log.Warning($"Lost actor: {this}");
						this.SetInvalid();
					}

					this.IsValid = false;
					this.IsRetargeting = false;
				}
			}

			private void UpdateActorInfo()
			{
				if (this.Memory == null)
					return;

				this.Id = this.Memory.Id;
				this.IdNoAddress = this.Memory.IdNoAddress;
				this.Name = this.Memory.Name;
				this.Memory.OnRetargeted();
				this.Memory.PropertyChanged += this.OnViewModelPropertyChanged;
				this.Pointer = this.Memory.Address;
				this.Kind = this.Memory.ObjectKind;
				this.ModelType = this.Memory.ModelType;
				this.IsGPoseActor = this.Memory.IsGPoseActor;
				this.IsHidden = this.Memory.IsHidden;

				this.UpdateInitials(this.DisplayName);

				this.IsValid = true;
			}

			private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (this.Memory != null && e.PropertyName == nameof(ActorMemory.DisplayName))
				{
					this.UpdateInitials(this.Memory.DisplayName);
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

						string[] parts = name.Split('(', StringSplitOptions.RemoveEmptyEntries);
						parts = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
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