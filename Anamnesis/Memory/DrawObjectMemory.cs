// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class DrawObjectMemory : SceneObjectMemory
{
	[Bind(0x88)] public byte Flags { get; set; }

	public bool IsVisible
	{
		get => (this.Flags & 0x09) == 0x09;
		set => this.Flags = (byte)(value ? this.Flags | 0x09 : this.Flags & ~0x09);
	}
}
