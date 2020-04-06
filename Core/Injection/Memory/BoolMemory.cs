// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class BoolMemory : MemoryBase<bool>
	{
		public BoolMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override bool Read()
		{
			return this.ReadByte() == 1;
		}

		protected override void Write(bool value)
		{
			this.WriteByte((byte)(value ? 1 : 0));
		}
	}
}
