// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Numerics;
using System.Runtime.CompilerServices;
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

	/// <summary>
	/// Builds a rigid (unit-scale) 4x4 matrix from this transform's rotation and translation.
	/// </summary>
	/// <param name="translationOverride">An optional translation to use instead of <see cref="Translation"/>.</param>
	/// <returns>A rigid <see cref="Matrix4x4"/> with unit scale.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Matrix4x4 ToRigidMatrix(Vector3? translationOverride = null)
	{
		Vector3 pos = translationOverride ?? this.Translation.ToVector3();
		Matrix4x4 mat = Matrix4x4.CreateFromQuaternion(this.Rotation.ToNormalizedQuaternion());
		mat.M41 = pos.X;
		mat.M42 = pos.Y;
		mat.M43 = pos.Z;
		return mat;
	}
}
