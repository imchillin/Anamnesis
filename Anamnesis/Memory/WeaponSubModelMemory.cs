// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class WeaponSubModelMemory : MemoryBase
{
	[Bind(0x070)] public Vector Scale { get; set; }
	[Bind(0x260)] public Color Tint { get; set; }

	public bool IsHidden
	{
		get => this.Scale == Vector.Zero;
		set => this.Scale = value ? Vector.Zero : Vector.One;
	}

	public void Hide()
	{
		this.IsHidden = true;
	}
}
