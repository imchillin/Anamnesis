// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class SkeletonMemory : ArrayMemory<PartialSkeletonMemory, short>
{
	public override int AddressOffset => 0x068;
	public override int CountOffset => 0x050;
	public override int ElementSize => 0x220;
}
