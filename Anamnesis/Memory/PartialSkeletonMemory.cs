// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;
public class PartialSkeletonMemory : MemoryBase
{
	[Bind(0x12C)] public short ConnectedBoneIndex { get; set; }
	[Bind(0x12E)] public short ConnectedParentBoneIndex { get; set; }

	[Bind(0x140, BindFlags.Pointer)] public HkaPoseMemory? Pose1 { get; set; }
	[Bind(0x148, BindFlags.Pointer)] public HkaPoseMemory? Pose2 { get; set; }
	[Bind(0x150, BindFlags.Pointer)] public HkaPoseMemory? Pose3 { get; set; }
	[Bind(0x158, BindFlags.Pointer)] public HkaPoseMemory? Pose4 { get; set; }
}
