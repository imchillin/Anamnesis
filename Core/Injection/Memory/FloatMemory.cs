// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class FloatMemory : MemoryBase<float>
	{
		public FloatMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override float Read()
		{
			byte[] bytearray = this.ReadBytes(4);
			return BitConverter.ToSingle(bytearray, 0);
		}

		protected override void Write(float value)
		{
			this.WriteBytes(BitConverter.GetBytes(value));
		}
	}
}
