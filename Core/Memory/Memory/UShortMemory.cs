// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Memory
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class UShortMemory : MemoryBase<ushort>
	{
		public UShortMemory(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 2)
		{
		}

		protected override ushort Read(ref byte[] data)
		{
			return BitConverter.ToUInt16(data, 0);
		}

		protected override void Write(ushort value, ref byte[] data)
		{
			// TODO: GetBytes creates a new 4 byte array.
			// consider getting the ushort into data directly... somehow.
			Array.Copy(BitConverter.GetBytes(value), data, 2);
		}
	}
}
