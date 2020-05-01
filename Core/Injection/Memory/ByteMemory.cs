// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class ByteMemory : MemoryBase<byte>
	{
		public ByteMemory(IProcess process, UIntPtr address)
			: base(process, address, 1)
		{
		}

		protected override byte Read(ref byte[] data)
		{
			return data[0];
		}

		protected override void Write(byte value, ref byte[] data)
		{
			data[0] = value;
		}
	}
}
