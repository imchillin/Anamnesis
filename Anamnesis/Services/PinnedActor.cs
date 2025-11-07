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
using System.Collections.Generic;
using System.ComponentModel;

[AddINotifyPropertyChangedInterface]
public class PinnedActor : INotifyPropertyChanged, IDisposable
{
	private bool isRestoringBackup = false;

	public PinnedActor(ActorMemory memory)
	{
		this.Id = memory.Id;
		this.IdNoAddress = memory.IdNoAddress;
		this.Memory = memory;
		this.Memory.Pinned = this;

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

	public ActorMemory? Memory { get; private set; }
	public string? Name { get; private set; }
	public string Id { get; private set; }
	public string IdNoAddress { get; private set; }
	public IntPtr? Pointer { get; private set; }
	public IconChar Icon { get; private set; }
	public string? Initials { get; private set; }
	public bool IsValid { get; private set; }

	[DependsOn(nameof(Memory))]
	public ActorTypes Kind => this.Memory?.ObjectKind ?? ActorTypes.None;

	[DependsOn(nameof(Memory))]
	public bool IsGPoseActor => this.Memory?.IsGPoseActor ?? false;

	[DependsOn(nameof(Memory))]
	public string? DisplayName => this.Memory == null ? this.Name : this.Memory.DisplayName;

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
			this.Memory.PropertyChanged -= this.OnViewModelPropertyChanged;
			this.Memory.Pinned = null;
			this.Memory = null;
		}

		GC.SuppressFinalize(this);
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

			if (this.Memory.IsGPoseActor && !GposeService.Instance.IsGpose)
			{
				Log.Information($"Actor: {this} was a gpose actor and we are now in the overworld");
				this.Retarget();
				return;
			}

			if (this.Memory.IsHidden && !this.Memory.IsGPoseActor && !this.Memory.IsRefreshing && GposeService.Instance.IsGpose)
			{
				Log.Information($"Actor: {this} was hidden entering the gpose boundary");
				this.Retarget();
				return;
			}

			try
			{
				this.Memory.Synchronize();
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

		if (this.Memory == null)
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

		ActorMemory? memory = this.GetMemory();
		if (memory == null)
		{
			Log.Warning("Unable to create character backup, pinned actor has no memory");
			return;
		}

		this.isRestoringBackup = true;
		backup.Apply(memory, slot == null ? CharacterFile.SaveModes.All : CharacterFile.SaveModes.EquipmentSlot, slot);

		// If we were a player, really make sure we are again.
		if (backup.ObjectKind == ActorTypes.Player)
		{
			memory.ObjectKind = backup.ObjectKind;
		}

		this.isRestoringBackup = false;
	}

	private void SetInvalid()
	{
		if (this.Memory != null)
		{
			this.Memory.Pinned = null;
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

			if (this.Memory != null)
				this.Memory.PropertyChanged -= this.OnViewModelPropertyChanged;

			ActorBasicMemory? newBasic = null;
			bool isGPose = GposeService.GetIsGPose();

			List<ActorBasicMemory> allActors = ActorService.Instance.GetAllActors();

			// As we do not actively update non-pinned actors, synchronize first
			foreach (ActorBasicMemory actor in allActors)
				actor.Synchronize();

			// Search for an exact match first
			foreach (ActorBasicMemory actor in allActors)
			{
				if (!actor.IsValid)
					continue;

				if (actor.Id != this.Id || actor.Address == IntPtr.Zero)
					continue;

				// Don't consider hidden actors for retargeting
				if (actor.IsHidden)
					continue;

				if (actor.IsGPoseActor != isGPose)
					continue;

				newBasic = actor;
				break;
			}

			// fall back to ignoring addresses
			if (newBasic == null)
			{
				foreach (ActorBasicMemory actor in allActors)
				{
					if (!actor.IsValid)
						continue;

					if (actor.IdNoAddress != this.IdNoAddress || actor.Address == IntPtr.Zero)
						continue;

					// Don't consider hidden actors for retargeting
					if (actor.IsHidden)
						continue;

					if (actor.IsGPoseActor != isGPose)
						continue;

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
				this.Memory ??= new ActorMemory();
				this.Memory.SetAddress(newBasic.Address);

				IntPtr? oldPointer = this.Pointer;

				this.UpdateActorInfo();

				// dont log every time we just select an actor.
				if (oldPointer != null && oldPointer != this.Pointer)
				{
					Log.Information($"Retargeted actor: {this} from {oldPointer} to {this.Pointer}");
				}

				if (this.IsSelected)
				{
					TargetService.Instance.SelectActor(this);
				}

				this.IsRetargeting = false;

				this.OnRetargetedActor();

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
		this.Pointer = this.Memory.Address;
		this.Icon = this.Memory.ObjectKind.GetIcon();

		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Kind)));
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsGPoseActor)));

		this.Memory.PropertyChanged += this.OnViewModelPropertyChanged;

		this.UpdateInitials(this.DisplayName);

		this.IsValid = true;
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (this.Memory == null)
			return;

		if (e.PropertyName == nameof(ActorMemory.DisplayName))
		{
			this.UpdateInitials(this.Memory.DisplayName);
			return;
		}

		if (e.PropertyName == nameof(ActorMemory.ObjectKind))
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Kind)));
			this.Icon = this.Memory.ObjectKind.GetIcon();
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
		if (SettingsService.Current.ReapplyAppearance || GposeService.GetIsGPose())
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