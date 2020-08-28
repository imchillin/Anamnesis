// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;
	using Anamnesis.Memory.Process;

	internal class UShortMarshaler : MarshalerBase<ushort>
	{
		public UShortMarshaler(IProcess process, IMemoryOffset[] offsets)
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
