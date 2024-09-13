// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Numerics;

public class WeaponSubModelMemory : MemoryBase
{
	[Bind(0x070)] public Vector3 Scale { get; set; }
	[Bind(0x290)] public Color Tint { get; set; }

	public bool IsHidden
	{
		get => this.Scale == Vector3.Zero;
		set => this.Scale = value ? Vector3.Zero : Vector3.One;
	}

	public void Hide()
	{
		this.IsHidden = true;
	}
}
