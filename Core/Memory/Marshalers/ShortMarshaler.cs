// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Marshalers
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class ShortMarshaler : MarshalerBase<short>
	{
		public ShortMarshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 2)
		{
		}

		protected override short Read(ref byte[] data)
		{
			return BitConverter.ToInt16(data, 0);
		}

		protected override void Write(short value, ref byte[] data)
		{
			// TODO: GetBytes creates a new 4 byte array.
			// consider getting the ushort into data directly... somehow.
			Array.Copy(BitConverter.GetBytes(value), data, 2);
		}
	}
}
