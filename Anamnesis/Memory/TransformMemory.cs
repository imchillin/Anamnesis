// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class TransformMemory : MemoryBase, ITransform
{
	[Bind(0x000)] public Vector Position { get; set; }
	[Bind(0x010)] public Quaternion Rotation { get; set; }
	[Bind(0x020)] public Vector Scale { get; set; }

	public bool CanTranslate => true;
	public bool CanRotate => true;
	public bool CanScale => true;
	public bool CanLinkScale => true;
	public bool ScaleLinked { get; set; } = true;

	public Quaternion RootRotation => Quaternion.Identity;
}
