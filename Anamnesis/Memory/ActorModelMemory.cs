// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.GameData;
using System;
using System.ComponentModel;

public class ActorModelMemory : DrawObjectMemory
{
	[Bind(0x030, BindFlags.Pointer)] public new ExtendedWeaponMemory? ChildObject { get; set; }
	[Bind(0x0A0, BindFlags.Pointer | BindFlags.OnlyInGPose | BindFlags.DontCacheOffsets)] public SkeletonMemory? Skeleton { get; set; }
	[Bind(0x150, BindFlags.Pointer)] public BustMemory? Bust { get; set; }
	[Bind(0x290)] public Color Tint { get; set; }
	[Bind(0x2A4)] public float Height { get; set; }
	[Bind(0x2E0)] public float Wetness { get; set; }
	[Bind(0x2EC)] public float Drenched { get; set; }
	[Bind(0xAA0)] public short DataPath { get; set; }
	[Bind(0xAA4)] public byte DataHead { get; set; }
	[Bind(0xBF0, 0x024)] public int ExtendedAppearanceFlags { get; private set; }
	[Bind(0xBF0, 0x028, BindFlags.Pointer)] public ExtendedAppearanceMemory? ExtendedAppearanceUnsafePtr { get; private set; }

	public bool LockWetness
	{
		get => this.IsFrozen(nameof(ActorModelMemory.Wetness));
		set => this.SetFrozen(nameof(ActorModelMemory.Wetness), value);
	}

	public bool ForceDrenched
	{
		get => this.IsFrozen(nameof(ActorModelMemory.Drenched));
		set => this.SetFrozen(nameof(ActorModelMemory.Drenched), value, value ? 5 : 0);
	}

	public bool IsHuman
	{
		get
		{
			if (!Enum.IsDefined(typeof(DataPaths), this.DataPath))
				return false;

			if (this.Parent is ActorMemory actor)
				return actor.ModelType == 0;

			return false;
		}
	}

	public ExtendedAppearanceMemory? ExtendedAppearance
		=> (this.IsHuman && (this.ExtendedAppearanceFlags & 0x4003) == 0) ? this.ExtendedAppearanceUnsafePtr : null;

	protected override bool CanRead(BindInfo bind)
	{
		if (bind.Name == nameof(this.ExtendedAppearanceFlags) ||
			bind.Name == nameof(this.ExtendedAppearanceUnsafePtr))
		{
			// No extended appearance for anything that isn't a player!
			if (!this.IsHuman)
			{
				if (this.ExtendedAppearanceUnsafePtr != null)
				{
					this.ExtendedAppearanceUnsafePtr?.Dispose();
					this.ExtendedAppearanceUnsafePtr = null;
				}

				return false;
			}
		}

		return base.CanRead(bind);
	}

	protected override void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.Height) && this.Height <= 0)
		{
			this.Height = 0.1f;
		}

		base.OnSelfPropertyChanged(sender, e);
	}
}
