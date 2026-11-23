// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using PropertyChanged;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public struct TransformStruct : IEquatable<TransformStruct>
{
	[FieldOffset(0x000)]
	public Vector3 Position;

	[FieldOffset(0x010)]
	public Quaternion Rotation;

	[FieldOffset(0x020)]
	public Vector3 Scale;

	public static bool operator ==(TransformStruct left, TransformStruct right) => left.Equals(right);

	public static bool operator !=(TransformStruct left, TransformStruct right) => !left.Equals(right);

	public readonly bool Equals(TransformStruct other)
		=> this.Position == other.Position
		&& this.Rotation == other.Rotation
		&& this.Scale == other.Scale;

	public override readonly bool Equals(object? obj) => obj is TransformStruct other && this.Equals(other);

	public override readonly int GetHashCode() => HashCode.Combine(this.Position, this.Rotation, this.Scale);
}

public class TransformMemory : MemoryBase, ITransform
{
	private readonly Lock transformLock = new();
	private TransformStruct transformStruct;

	public static Quaternion RootRotation => Quaternion.Identity;

	[AlsoNotifyFor(nameof(Position), nameof(Rotation), nameof(Scale))]
	[Bind(0x000)]
	public TransformStruct Transform
	{
		get
		{
			lock (this.transformLock)
			{
				return this.transformStruct;
			}
		}
		set
		{
			lock (this.transformLock)
			{
				this.transformStruct = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(Transform))]
	public Vector3 Position
	{
		get
		{
			lock (this.transformLock)
			{
				return this.transformStruct.Position;
			}
		}
		set
		{
			lock (this.transformLock)
			{
				this.transformStruct.Position = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(Transform))]
	public Quaternion Rotation
	{
		get
		{
			lock (this.transformLock)
			{
				return this.transformStruct.Rotation;
			}
		}
		set
		{
			lock (this.transformLock)
			{
				this.transformStruct.Rotation = value;
			}
		}
	}

	[AlsoNotifyFor(nameof(Transform))]
	public Vector3 Scale
	{
		get
		{
			lock (this.transformLock)
			{
				return this.transformStruct.Scale;
			}
		}
		set
		{
			lock (this.transformLock)
			{
				this.transformStruct.Scale = value;
			}
		}
	}

	public bool CanTranslate => true;
	public bool CanRotate => true;
	public bool CanScale => true;
	public bool CanLinkScale => true;
	public bool ScaleLinked { get; set; } = true;
}
