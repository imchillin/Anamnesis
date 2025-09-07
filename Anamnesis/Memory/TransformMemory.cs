// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Numerics;

public class TransformMemory : MemoryBase, ITransform
{
	public static Quaternion RootRotation => Quaternion.Identity;

	[Bind(0x000)] public Vector3 Position { get; set; }
	[Bind(0x010)] public Quaternion Rotation { get; set; }
	[Bind(0x020)] public Vector3 Scale { get; set; }

	public bool CanTranslate => true;
	public bool CanRotate => true;
	public bool CanScale => true;
	public bool CanLinkScale => true;
	public bool ScaleLinked { get; set; } = true;
}
