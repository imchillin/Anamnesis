// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Represents the memory structure for Havok skeletons in FFXIV.
/// </summary>
public class HkaSkeletonMemory : MemoryBase
{
	/// <summary>Gets or sets the name of the skeleton.</summary>
	/// <remarks>This is a pointer to a UTF-8 string.</remarks>
	[Bind(0x010, BindFlags.Pointer | BindFlags.DontCacheOffsets)] public Utf8String Name { get; set; }

	/// <summary>Gets or sets the array of parent indices for the bones.</summary>
	[Bind(0x018, BindFlags.DontCacheOffsets)] public ParentingArrayMemory? ParentIndices { get; set; }

	/// <summary>Gets or sets the array of bones in the skeleton.</summary>
	[Bind(0x028, BindFlags.DontCacheOffsets)] public BoneArrayMemory? Bones { get; set; }

	/// <summary>Represents an array of parent indices for the bones in the skeleton.</summary>
	public class ParentingArrayMemory : HkaArrayMemory<short>
	{
		public override int ElementSize => 0x002;
	}

	/// <summary>Represents an array of bones in the skeleton.</summary>
	public class BoneArrayMemory : HkaArrayMemory<HkaBone>
	{
		public override int ElementSize => 0x010;
	}

	/// <summary>
	/// Represents a generic array memory structure for Havok arrays.
	/// </summary>
	/// <typeparam name="T">The type of elements in the array.</typeparam>
	public abstract class HkaArrayMemory<T> : ArrayMemory<T, int>
	{
		public override int AddressOffset => 0x000;
		public override int LengthOffset => 0x008;
	}

	/// <summary>
	/// Represents a bone in the Havok skeleton.
	/// </summary>
	public class HkaBone : MemoryBase
	{
		/// <summary>Gets or sets the name of the bone.</summary>
		/// <remarks>This is a pointer to a UTF-8 string.</remarks>
		[Bind(0x000, BindFlags.Pointer | BindFlags.DontCacheOffsets)] public Utf8String Name { get; set; }
	}
}
