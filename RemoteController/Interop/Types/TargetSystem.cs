// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x6EF0)]
public struct TargetSystem
{
	[FieldOffset(0x98)] public nint GPoseTarget;
}
