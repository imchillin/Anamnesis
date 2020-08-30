// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;

	internal class ByteMarshaler : MarshalerBase<byte>
	{
		public ByteMarshaler(params IMemoryOffset[] offsets)
			: base(offsets, 1)
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
