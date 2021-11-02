// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public class SkeletonMemory : ArrayMemory<PartialSkeletonMemory, int>
	{
		public override int AddressOffset => 0x068;
		public override int CountOffset => 0x050;
	}
}