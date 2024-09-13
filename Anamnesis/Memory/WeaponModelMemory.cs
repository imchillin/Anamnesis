// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Represents the memory structure for a weapon model in FFXIV.
/// </summary>
public class WeaponModelMemory : MemoryBase
{
	/// <summary>
	/// Gets or sets the weapon model's transform memory (position, rotation, scale).
	/// </summary>
	[Bind(0x050)] public TransformMemory? Transform { get; set; }

	/// <summary>
	/// Gets or sets the skeleton memory of the weapon model.
	/// </summary>
	/// <remarks>
	/// Available only in GPose. Internally an array of <see cref="PartialSkeletonMemory"/> elements.
	/// Offsets are not cached to avoid issues.
	/// </remarks>
	[Bind(0x0A0, BindFlags.Pointer | BindFlags.OnlyInGPose | BindFlags.DontCacheOffsets)] public SkeletonMemory? Skeleton { get; set; }
}
