// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class BoolMemory : MemoryBase<bool>
	{
		public BoolMemory(IProcess process, UIntPtr address)
			: base(process, address, 1)
		{
		}

		protected override bool Read(ref byte[] data)
		{
			return data[0] == 1;
		}

		protected override void Write(bool value, ref byte[] data)
		{
			data[0] = (byte)(value ? 1 : 0);
		}
	}
}
