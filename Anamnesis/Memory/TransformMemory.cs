// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using PropertyChanged;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public struct TransformStruct
{
	[FieldOffset(0x000)]
	public Vector3 Position;

	[FieldOffset(0x010)]
	public Quaternion Rotation;

	[FieldOffset(0x020)]
	public Vector3 Scale;
}

public class TransformMemory : MemoryBase, ITransform
{
	private readonly Lock transformLock = new();
	private TransformStruct transformStruct;

	public static Quaternion RootRotation => Quaternion.Identity;

	[AlsoNotifyFor(nameof(Position), nameof(Rotation), nameof(Scale))]
	[Bind(0x000)] public TransformStruct Transform
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
