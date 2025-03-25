// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;
public class PartialSkeletonMemory : MemoryBase
{
	[Bind(0x120, BindFlags.DontCacheOffsets)] public short ConnectedParentBoneIndex { get; set; }
	[Bind(0x122, BindFlags.DontCacheOffsets)] public short ConnectedBoneIndex { get; set; }

	[Bind(0x148, BindFlags.Pointer | BindFlags.DontCacheOffsets)] public HkaPoseMemory? Pose1 { get; set; }
	[Bind(0x150, BindFlags.Pointer | BindFlags.DontCacheOffsets)] public HkaPoseMemory? Pose2 { get; set; }
	[Bind(0x158, BindFlags.Pointer | BindFlags.DontCacheOffsets)] public HkaPoseMemory? Pose3 { get; set; }
	[Bind(0x160, BindFlags.Pointer | BindFlags.DontCacheOffsets)] public HkaPoseMemory? Pose4 { get; set; }
}
