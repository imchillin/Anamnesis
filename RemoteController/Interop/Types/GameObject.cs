// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x01A0)]
public struct GameObject
{
	public const int DRAW_OBJECT_OFFSET = 0x0100;

	[FieldOffset(0x100)]
	public unsafe DrawObject* ModelObject;
}
