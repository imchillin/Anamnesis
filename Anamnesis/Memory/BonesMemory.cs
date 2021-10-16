// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class BonesMemory : MemoryBase
	{
		////[Bind(0x000, BindFlags.Pointer)] public IntPtr HkAnimationFile;
		[Bind(0x010)] public int Count { get; set; }
		////[Bind(0x018, BindFlags.Pointer)] public IntPtr TransformArray { get; set; }
	}
}
