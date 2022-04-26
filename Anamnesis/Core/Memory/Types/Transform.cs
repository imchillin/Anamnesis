// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct Transform
{
	[FieldOffset(0x00)]
	public Vector Position;

	[FieldOffset(0x10)]
	public Quaternion Rotation;

	[FieldOffset(0x20)]
	public Vector Scale;
}
