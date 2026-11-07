// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct UIModule
{
	[FieldOffset(0xD2690)] public AtkModule RaptureAtkModule;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Framework
{
	[FieldOffset(0x10)] public SystemConfig SystemConfig;
	[FieldOffset(0x2B68)] public UIModule* UIModule;
}
