// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class IntMemory : MemoryBase<int>
	{
		public IntMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override int Read()
		{
			byte[] b = this.ReadBytes(1);
			return (int)b[0];
		}

		protected override void Write(int value)
		{
			byte b = Convert.ToByte(value);
			this.WriteBytes(new[] { b });
		}
	}
}
