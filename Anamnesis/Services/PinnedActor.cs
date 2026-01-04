// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles;
using FontAwesome.Sharp;
using PropertyChanged;
using Serilog;
using System;
using System.ComponentModel;

[AddINotifyPropertyChangedInterface]
public class PinnedActor : INotifyPropertyChanged, IDisposable
{
	private bool isRestoringBackup = false;

	public PinnedActor(ObjectHandle<ActorMemory> handle)
	{
		if (!handle.IsValid)
			throw new ArgumentException("Cannot pin an invalid actor handle");

		this.Id = handle.DoRef(a => a.Id) ?? string.Empty;
		this.IdNoAddress = handle.DoRef(a => a.IdNoAddress) ?? string.Empty;
		this.Name = handle.DoRef(a => a.Name);
		this.Memory = handle;
		handle.Do(a => a.Pinned = this);

		this.UpdateActorInfo();

		GposeService.GposeStateChanged += this.OnGposeStateChanged;

		this.CreateCharacterBackup(BackupModes.Original);
		this.CreateCharacterBackup(BackupModes.Gpose);
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event TargetService.PinnedEvent? Retargeted;

	public enum BackupModes
	{
		Gpose,
		Original,
	}

	public ObjectHandle<ActorMemory>? Memory { get; private set; }
	public string? Name { get; private set; }
	public string Id { get; private set; }
	public string IdNoAddress { get; private set; }
	public IntPtr? Pointer { get; private set; }
	public IconChar Icon { get; private set; }
	public string? Initials { get; private set; }
	public bool IsValid { get; private set; }

	[DependsOn(nameof(Memory))]
	public ActorTypes Kind => this.Memory?.Do(a => a.ObjectKind) ?? ActorTypes.None;

	[DependsOn(nameof(Memory))]
	public bool IsGPoseActor => this.Memory?.Do(a => a.IsGPoseActor) ?? false;

	[DependsOn(nameof(Memory))]
	public string? DisplayName => this.Memory?.DoRef(a => a.DisplayName) ?? this.Name;

	public bool IsRetargeting { get; private set; } = false;

	public CharacterFile? GposeCharacterBackup { get; private set; }
	public CharacterFile? OriginalCharacterBackup { get; private set; }

	public bool IsSelected
	{
		get => TargetService.Instance.CurrentlyPinned == this;
		set
		{
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
		GposeService.GposeStateChanged -= this.OnGposeStateChanged;

		if (this.Memory != null)
		{
			this.Memory.Do(a =>
			{
				a.PropertyChanged -= this.OnViewModelPropertyChanged;
				a.Pinned = null;
			});
			this.Memory.Dispose();
			this.Memory = null;
		}

		GC.SuppressFinalize(this);
	}

	public void SelectionChanged()
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
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

			if (!this.IsValid || this.Memory == null || !this.Memory.IsValid)
			{
				this.Retarget();
				return;
			}

			if (!ActorService.Instance.ObjectTable.Contains(this.Memory.Address))
			{
				Log.Information($"Actor: {this} was not in actor table");
				this.Retarget();
				return;
			}

			if (this.Memory.Do(a => a.IsGPoseActor) == true && !GposeService.Instance.IsGpose)
			{
				Log.Information($"Actor: {this} was a gpose actor and we are now in the overworld");
				this.Retarget();
				return;
			}

			if (this.Memory.Do(a => a.IsHidden && !a.IsGPoseActor && !a.IsRefreshing) == true && GposeService.Instance.IsGpose)
			{
				Log.Information($"Actor: {this} was hidden entering the gpose boundary");
				this.Retarget();
				return;
			}

			try
			{
				this.Memory.Do(a => a.Synchronize());
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Failed to tick actor");
				this.SetInvalid();
			}
		}
	}

	public void CreateCharacterBackup(BackupModes mode)
	{
		if (this.isRestoringBackup)
			return;

		if (this.Memory == null || !this.Memory.IsValid)
		{
			Log.Warning("Unable to create character backup, pinned actor has no memory");
			return;
		}

		if (mode == BackupModes.Original)
		{
			this.OriginalCharacterBackup ??= new();
			this.OriginalCharacterBackup.WriteToFile(this.Memory, CharacterFile.SaveModes.All);
		}
		else if (mode == BackupModes.Gpose)
		{
			this.GposeCharacterBackup ??= new();
			this.GposeCharacterBackup.WriteToFile(this.Memory, CharacterFile.SaveModes.All);
		}
	}

	public void RestoreCharacterBackup(BackupModes mode, ItemSlots? slot = null)
	{
		CharacterFile? backup = null;

		if (mode == BackupModes.Gpose)
		{
			backup = this.GposeCharacterBackup;
		}
		else if (mode == BackupModes.Original)
		{
			backup = this.OriginalCharacterBackup;
		}

		if (backup == null)
			return;

		if (this.Memory == null)
		{
			Log.Warning("Unable to create character backup, pinned actor is not valid");
			return;
		}

		this.isRestoringBackup = true;
		backup.Apply(this.Memory, slot == null ? CharacterFile.SaveModes.All : CharacterFile.SaveModes.EquipmentSlot, slot);

		// If we were a player, really make sure we are again.
		if (backup.ObjectKind == ActorTypes.Player)
		{
			this.Memory.Do(a => a.ObjectKind = backup.ObjectKind);
		}

		this.isRestoringBackup = false;
	}

	private void SetInvalid()
	{
		if (this.Memory != null)
		{
			this.Memory.Do(a => a.Pinned = null);
			this.Memory.Dispose();
			this.Memory = null;

			if (this.IsSelected)
				TargetService.Instance.SelectActor(this);
		}

		this.IsValid = false;
	}

	private void Retarget()
	{
#if DEBUG
		if (MemoryService.Process == null)
		{
			this.IsValid = true;
			this.IsRetargeting = false;
			return;
		}
#endif

		lock (this)
		{
			this.IsRetargeting = true;

			this.Memory?.Do(a => a.PropertyChanged -= this.OnViewModelPropertyChanged);

			ObjectHandle<GameObjectMemory>? newHandle = null;
			bool isGPose = GposeService.IsInGpose();

			var actorHandles = ActorService.Instance.ObjectTable.GetAll();

			// As we do not actively update non-pinned actors, synchronize first
			foreach (var handle in actorHandles)
			{
				handle.Do(a => a.Synchronize());
			}

			// Search for an exact match first
			foreach (var handle in actorHandles)
			{
				var result = handle.Do(a =>
				{
					if (!a.IsValid)
						return false;

					if (a.Id != this.Id || a.Address == IntPtr.Zero)
						return false;

					// Don't consider hidden actors for retargeting
					if (a.IsHidden)
						return false;

					if (a.IsGPoseActor != isGPose)
						return false;

					return true;
				});

				if (result == true)
				{
					newHandle = handle;
					break;
				}
			}

			// Fallback to ignoring the address for retargeting
			if (newHandle == null)
			{
				foreach (var handle in actorHandles)
				{
					var result = handle.Do(a =>
					{
						if (!a.IsValid)
							return false;

						if (a.IdNoAddress != this.IdNoAddress || a.Address == IntPtr.Zero)
							return false;

						// Don't consider hidden actors for retargeting
						if (a.IsHidden)
							return false;

						if (a.IsGPoseActor != isGPose)
							return false;

						// Is this actor memory already pinned to a different pin?
						PinnedActor? pinned = TargetService.GetPinned(handle);
						if (pinned != this && pinned != null)
							return false;

						return true;
					});

					if (result == true)
					{
						newHandle = handle;
						break;
					}
				}
			}

			if (newHandle != null)
			{
				IntPtr? oldAddress = this.Memory?.Address;
				this.Memory?.Dispose();
				this.Memory = ActorService.Instance.ObjectTable.Get<ActorMemory>(newHandle.Address);
				this.Memory?.Do(a => a.Pinned = this);

				this.UpdateActorInfo();

				// Dont log every time we just select an actor.
				if (oldAddress != null && oldAddress != this.Pointer)
					Log.Information($"Retargeted actor: {this} from {oldAddress} to {this.Pointer}");

				if (this.IsSelected)
					TargetService.Instance.SelectActor(this);

				this.IsRetargeting = false;
				this.OnRetargetedActor();
				return;
			}

			if (this.Memory?.IsValid == true)
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

		this.Id = this.Memory.DoRef(a => a.Id) ?? string.Empty;
		this.IdNoAddress = this.Memory.DoRef(a => a.IdNoAddress) ?? string.Empty;
		this.Name = this.Memory.DoRef(a => a.Name);
		this.Pointer = this.Memory.Address;

		var objectKind = this.Memory.Do(a => a.ObjectKind);
		if (objectKind != null)
		{
			this.Icon = ((ActorTypes)objectKind).GetIcon();
		}

		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Kind)));
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsGPoseActor)));

		this.Memory.Do(a => a.PropertyChanged += this.OnViewModelPropertyChanged);

		this.UpdateInitials(this.DisplayName);

		this.IsValid = true;
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (this.Memory == null)
			return;

		if (e.PropertyName == nameof(ActorMemory.DisplayName))
		{
			this.UpdateInitials(this.Memory.DoRef(a => a.DisplayName));
			return;
		}

		if (e.PropertyName == nameof(ActorMemory.ObjectKind))
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Kind)));
			var objectKind = this.Memory.Do(a => a.ObjectKind);
			if (objectKind != null)
			{
				this.Icon = ((ActorTypes)objectKind).GetIcon();
			}

			return;
		}

		if (e.PropertyName == nameof(ActorMemory.IsGPoseActor))
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsGPoseActor)));
			return;
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

	private void OnRetargetedActor()
	{
		// If we need to apply the appearance thanks to a GPose boundary changes?
		if (SettingsService.Current.ReapplyAppearance || GposeService.IsInGpose())
		{
			this.RestoreCharacterBackup(BackupModes.Gpose);

			// clear the appearance once it has been applied
			if (!SettingsService.Current.ReapplyAppearance)
			{
				this.GposeCharacterBackup = null;
			}
		}

		this.Retargeted?.Invoke(this);
	}

	private void OnGposeStateChanged(bool newState)
	{
		if (newState)
		{
			this.CreateCharacterBackup(BackupModes.Gpose);
		}
	}
}