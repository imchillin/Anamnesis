// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public struct HkaTransform4
{
	[FieldOffset(0x00)]
	public HkaVector4 Translation;
	
	[FieldOffset(0x10)]
	public HkaQuaternion Rotation;
	
	[FieldOffset(0x20)]
	public HkaVector4 Scale;
}
