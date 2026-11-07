// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x18A0)]
public struct AtkInputManager
{
	[FieldOffset(0x1896)] public bool IsTextInputActive;
}

[StructLayout(LayoutKind.Explicit, Size = 0x82F0)]
public struct AtkModule
{
	[FieldOffset(0x2C8)] public AtkInputManager AtkInputManager;
}
