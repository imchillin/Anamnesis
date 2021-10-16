// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	// We dont know what this structure is.
	public class SkeletonWrapperMemory : MemoryBase
	{
		[Bind(0x068, BindFlags.Pointer)] public SkeletonMemory? Skeleton { get; set; }
	}
}
