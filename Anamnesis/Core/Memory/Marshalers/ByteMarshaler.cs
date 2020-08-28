// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;
	using Anamnesis.Memory.Process;

	internal class ByteMarshaler : MarshalerBase<byte>
	{
		public ByteMarshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 1)
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
