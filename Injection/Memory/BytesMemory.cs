// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class BytesMemory : MemoryBase<byte[]>
	{
		public long Length = 16;

		public BytesMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override byte[] Read()
		{
			return this.ReadBytes(this.Length);
		}

		protected override void Write(byte[] value)
		{
			this.WriteBytes(value);
		}
	}
}
