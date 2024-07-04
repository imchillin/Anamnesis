﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.ComponentModel;

public class ActorModelMemory : MemoryBase
{
	/// <summary>
	/// Known data paths.
	/// </summary>
	public enum DataPaths : short
	{
		MidlanderMasculine = 101,
		MidlanderMasculineChild = 104,
		MidlanderFeminine = 201,
		MidlanderFeminineChild = 204,
		HighlanderMasculine = 301,
		HighlanderFeminine = 401,
		ElezenMasculine = 501,
		ElezenMasculineChild = 504,
		ElezenFeminine = 601,
		ElezenFeminineChild = 604,
		MiqoteMasculine = 701,
		MiqoteMasculineChild = 704,
		MiqoteFeminine = 801,
		MiqoteFeminineChild = 804,
		RoegadynMasculine = 901,
		RoegadynFeminine = 1001,
		LalafellMasculine = 1101,
		LalafellFeminine = 1201,
		AuRaMasculine = 1301,
		AuRaFeminine = 1401,
		HrothgarMasculine = 1501,
		////HrothgarFeminine = 1601,
		VieraMasculine = 1701,
		VieraFeminine = 1801,
		PadjalMasculine = 9104,
		PadjalFeminine = 9204,
	}

	[Bind(0x030, BindFlags.Pointer)] public ExtendedWeaponMemory? Weapons { get; set; }
	[Bind(0x050)] public TransformMemory? Transform { get; set; }
	[Bind(0x0A0, BindFlags.Pointer | BindFlags.OnlyInGPose)] public SkeletonMemory? Skeleton { get; set; }
	[Bind(0x148, BindFlags.Pointer)] public BustMemory? Bust { get; set; }
	[Bind(0x290)] public Color Tint { get; set; }
	[Bind(0x2A4)] public float Height { get; set; }
	[Bind(0x2E0)] public float Wetness { get; set; }
	[Bind(0x2EC)] public float Drenched { get; set; }
	[Bind(0xA50)] public short DataPath { get; set; }
	[Bind(0xA54)] public byte DataHead { get; set; }
	[Bind(0xBA0, 0x028, BindFlags.Pointer)] public ExtendedAppearanceMemory? ExtendedAppearance { get; set; }

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
			{
				return actor.ModelType == 0;
			}

			return true;
		}
	}

	protected override bool CanRead(BindInfo bind)
	{
		if (bind.Name == nameof(ActorModelMemory.ExtendedAppearance))
		{
			// No extended appearance for anything that isn't a player!
			if (!this.IsHuman)
			{
				if (this.ExtendedAppearance != null)
				{
					this.ExtendedAppearance.Dispose();
					this.ExtendedAppearance = null;
				}

				return false;
			}
		}

		return base.CanRead(bind);
	}

	protected override void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if(e.PropertyName == nameof(this.Height) && this.Height <= 0)
		{
			this.Height = 0.1f;
		}

		base.OnSelfPropertyChanged(sender, e);
	}
}
