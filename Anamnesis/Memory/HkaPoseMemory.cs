// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public class HkaPoseMemory : MemoryBase
	{
		[Bind(0x000, BindFlags.Pointer)] public HkaSkeletonMemory? Skeleton { get; set; }
		[Bind(0x010)] public TransformArrayMemory? Transforms { get; set; }

		public class TransformArrayMemory : ArrayMemory<TransformMemory, int>
		{
			public override int CountOffset => 0x000;
			public override int AddressOffset => 0x008;
			public override int ElementSize => 0x030;
		}
	}
}