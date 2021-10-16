// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class BustMemory : MemoryBase
	{
		[Bind(0x068)] public Vector Scale { get; set; }
	}
}
