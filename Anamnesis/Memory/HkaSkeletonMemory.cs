// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class HkaSkeletonMemory : MemoryBase
	{
		[Bind(0x010, BindFlags.Pointer)] public Utf8String Name { get; set; }
		[Bind(0x018)] public IntPtr ParentIndices { get; set; }
		[Bind(0x020)] public int IndicesCount { get; set; }

		public override void Tick()
		{
			base.Tick();
		}
	}
}