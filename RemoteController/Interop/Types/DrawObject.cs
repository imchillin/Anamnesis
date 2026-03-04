// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x090)]
public struct DrawObject
{
	public const int FLAGS_OFFSET = 0x088;

	[FieldOffset(FLAGS_OFFSET)] public byte Flags;

	public bool IsVisible
	{
		readonly get => (this.Flags & 0x09) == 0x09;
		set => this.Flags = (byte)(value ? this.Flags | 0x09 : this.Flags & ~0x09);
	}
}
