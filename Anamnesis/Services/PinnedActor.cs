// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Anamnesis.Actor;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles;
using FontAwesome.Sharp;
using PropertyChanged;
using Serilog;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public class PinnedActor : INotifyPropertyChanged, IDisposable
{
	////private bool wasPlayer = false;
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
	public event PinnedEvent? Retargeted;

	public enum BackupModes
	{
		Gpose,
		Original,
	}

	public ActorMemory? Memory { get; private set; }
	////public ActorViewModel? ViewModel { get; private set; }

	public string? Name { get; private set; }
	public string Id { get; private set; }
	public string IdNoAddress { get; private set; }
	public IntPtr? Pointer { get; private set; }
	public ActorTypes Kind { get; private set; }
	public IconChar Icon { get; private set; }
	public int ModelType { get; private set; }
	public string? Initials { get; private set; }
	public bool IsValid { get; private set; }
	public bool IsGPoseActor { get; private set; }
	public bool IsHidden { get; private set; }
	public bool IsPinned => TargetService.Instance.PinnedActors.Contains(this);

	public string? DisplayName => this.Memory == null ? this.Name : this.Memory.DisplayName;
	public bool IsRetargeting { get; private set; } = false;

	public CharacterFile? GposeCharacterBackup { get; private set; }
	public CharacterFile? OriginalCharacterBackup { get; private set; }

	public bool IsSelected
	{
		get
		{
			return TargetService.Instance.CurrentlyPinned == this;
		}

		set
		{
			////if (!GameService.Instance.IsSignedIn)
			////	return;

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
			this.Memory.Pinned = null;
			this.Memory = null;
		}
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
				this.Memory.Tick();
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
			if (this.OriginalCharacterBackup == null)
				this.OriginalCharacterBackup = new();

			this.OriginalCharacterBackup.WriteToFile(this.Memory, CharacterFile.SaveModes.All);
		}
		else if (mode == BackupModes.Gpose)
		{
			if (this.GposeCharacterBackup == null)
				this.GposeCharacterBackup = new();

			this.GposeCharacterBackup.WriteToFile(this.Memory, CharacterFile.SaveModes.All);
		}
	}

	public async Task RestoreCharacterBackup(BackupModes mode)
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

		bool allowRefresh = !GposeService.GetIsGPose();
		await backup.Apply(memory, CharacterFile.SaveModes.All, allowRefresh);

		// If we were a player, really make sure we are again.
		if (allowRefresh && backup.ObjectKind == ActorTypes.Player)
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
					if (actor.IsHidden)
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
				if (this.Memory != null)
					this.Memory.Dispose();

				// Reusing the old actor can cause issues so we always recreate when retargeting.
				this.Memory = new ActorMemory();
				this.Memory.SetAddress(newBasic.Address);
				this.Memory.Pinned = this;

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

				this.OnRetargetedActor(oldPointer, this.Pointer);

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
		this.Memory.PropertyChanged += this.OnViewModelPropertyChanged;
		this.Pointer = this.Memory.Address;
		this.Kind = this.Memory.ObjectKind;
		this.Icon = this.Memory.ObjectKind.GetIcon();
		this.ModelType = this.Memory.ModelType;
		this.IsGPoseActor = this.Memory.IsGPoseActor;
		this.IsHidden = this.Memory.IsHidden;

		this.UpdateInitials(this.DisplayName);

		this.IsValid = true;
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (this.Memory == null)
			return;

		if (e.PropertyName == nameof(ActorMemory.DisplayName) || e.PropertyName == nameof(ActorMemory.ObjectKind))
		{
			this.UpdateInitials(this.Memory.DisplayName);
			this.Kind = this.Memory.ObjectKind;
			this.Icon = this.Memory.ObjectKind.GetIcon();
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

	private void OnRetargetedActor(IntPtr? oldPointer, IntPtr? newPointer)
	{
		/*if (GposeService.GetIsGPose() &&
			this.wasPlayer &&
			this.Memory != null &&
			this.Memory.ObjectKind != ActorTypes.Player)
		{
			IntPtr objectKindAddress = this.Memory.GetAddressOfProperty(nameof(ActorBasicMemory.ObjectKind));
			MemoryService.Write(objectKindAddress, ActorTypes.Player, "NPC face hack - entered gpose - gpose actor");
		}*/

		// If we need to apply the appearance thanks to a GPose boundary changes?
		if (SettingsService.Current.ReapplyAppearance || GposeService.GetIsGPose())
		{
			this.RestoreCharacterBackup(BackupModes.Gpose).Run();

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

			/*Task.Run(async () =>
			{
				if (this.Memory != null && this.Memory.ObjectKind == ActorTypes.Player)
				{
					this.wasPlayer = true;

					IntPtr objectKindAddress = this.Memory.GetAddressOfProperty(nameof(ActorBasicMemory.ObjectKind));
					MemoryService.Write(objectKindAddress, ActorTypes.BattleNpc, "NPC face hack - entering gpose - overworld actor");
					await Task.Delay(1000);
					MemoryService.Write(objectKindAddress, ActorTypes.Player, "NPC face hack - entered gpose - overworld actor");
				}
			});*/
		}
	}
}