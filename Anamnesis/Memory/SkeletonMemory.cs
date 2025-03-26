// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Represents the object's skeleton, composed of an array of <see cref="PartialSkeletonMemory"/> elements.
/// </summary>
public class SkeletonMemory : ArrayMemory<PartialSkeletonMemory, ushort>
{
	/// <inheritdoc/>
	public override int AddressOffset => 0x068;

	/// <inheritdoc/>
	public override int LengthOffset => 0x050;

	/// <inheritdoc/>
	public override int ElementSize => 0x230;
}
