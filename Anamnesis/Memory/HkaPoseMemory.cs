// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class HkaPoseMemory : MemoryBase
	{
		[Bind(0x000, BindFlags.Pointer)] public HkaSkeletonMemory? SkeletonPointer { get; set; }
	}
}