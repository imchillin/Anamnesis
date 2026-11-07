// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public struct Transform : IEquatable<Transform>
{
	[FieldOffset(0x00)]
	public Vector3 Position;

	[FieldOffset(0x10)]
	public Quaternion Rotation;

	[FieldOffset(0x20)]
	public Vector3 Scale;

	public static bool operator ==(Transform left, Transform right) => left.Equals(right);

	public static bool operator !=(Transform left, Transform right) => !left.Equals(right);

	public readonly bool Equals(Transform other)
	{
		ReadOnlySpan<byte> a = MemoryMarshal.AsBytes(new ReadOnlySpan<Transform>(in this));
		ReadOnlySpan<byte> b = MemoryMarshal.AsBytes(new ReadOnlySpan<Transform>(in other));
		return a.SequenceEqual(b);
	}

	public override readonly bool Equals(object? obj) => obj is Transform other && this.Equals(other);

	public override readonly int GetHashCode()
	{
		var hash = default(HashCode);
		hash.AddBytes(MemoryMarshal.AsBytes(new ReadOnlySpan<Transform>(in this)));
		return hash.ToHashCode();
	}
}
