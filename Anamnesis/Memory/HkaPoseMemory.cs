// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Represents the memory structure for a Havok pose in FFXIV.
/// </summary>
public class HkaPoseMemory : MemoryBase
{
	/// <summary>Gets or sets the skeleton memory of the Havok pose.</summary>
	[Bind(0x000, BindFlags.Pointer | BindFlags.DontCacheOffsets)] public HkaSkeletonMemory? Skeleton { get; set; }

	/// <summary>Gets or sets the transform array memory of the Havok pose.</summary>
	[Bind(0x018, BindFlags.DontCacheOffsets)] public TransformArrayMemory? Transforms { get; set; }
}

/// <summary>Represents an array of transform memories.</summary>
public class TransformArrayMemory : ArrayMemory<TransformMemory, int>
{
	public override int AddressOffset => 0x000;
	public override int LengthOffset => 0x008;
	public override int ElementSize => 0x030;
}
