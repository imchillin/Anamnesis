// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct Framework
{
	[FieldOffset(0x10)] public SystemConfig SystemConfig;
}
