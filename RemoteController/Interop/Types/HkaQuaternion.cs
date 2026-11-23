// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public struct HkaQuaternion(float x, float y, float z, float w)
{
	// IEEE-754 single-precision machine epsilon (C++ FLT_EPSILON = 2^-23), matching FFXIV/Havok's normalization guard.
	private const float NORMALIZE_EPSILON_SQ = 1.1920929e-7f;

	[FieldOffset(0x00)]
	public float X = x;
	[FieldOffset(0x04)]
	public float Y = y;
	[FieldOffset(0x08)]
	public float Z = z;
	[FieldOffset(0x0C)]
	public float W = w;

	/// <summary>
	/// Converts this Havok quaternion to a normalized <see cref="Quaternion"/>, falling back to identity if degenerate.
	/// </summary>
	/// <returns>A normalized <see cref="Quaternion"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Quaternion ToNormalizedQuaternion()
	{
		Quaternion q = new(this.X, this.Y, this.Z, this.W);
		return q.LengthSquared() > NORMALIZE_EPSILON_SQ ? Quaternion.Normalize(q) : Quaternion.Identity;
	}
}
