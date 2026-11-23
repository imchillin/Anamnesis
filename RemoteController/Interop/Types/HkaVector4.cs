// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public struct HkaVector4(float x, float y, float z, float w)
{
	[FieldOffset(0x00)]
	public float X = x;

	[FieldOffset(0x04)]
	public float Y = y;

	[FieldOffset(0x08)]
	public float Z = z;

	[FieldOffset(0x0C)]
	public float W = w;

	/// <summary>
	/// Converts the XYZ components to a <see cref="Vector3"/>.
	/// </summary>
	/// <returns>A <see cref="Vector3"/> with this vector's X, Y, and Z components.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Vector3 ToVector3() => new(this.X, this.Y, this.Z);
}
