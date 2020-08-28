// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Memory
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class FloatMemory : MemoryBase<float>
	{
		public FloatMemory(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 4)
		{
		}

		protected override float Read(ref byte[] data)
		{
			return BitConverter.ToSingle(data, 0);
		}

		protected override void Write(float value, ref byte[] data)
		{
			// TODO: GetBytes creates a new 4 byte array.
			// consider getting the float into data directly... somehow.
			Array.Copy(BitConverter.GetBytes(value), data, 4);
		}
	}
}
