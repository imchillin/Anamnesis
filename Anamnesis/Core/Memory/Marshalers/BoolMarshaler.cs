// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;

	internal class BoolMarshaler : MarshalerBase<bool>
	{
		public BoolMarshaler(params IMemoryOffset[] offsets)
			: base(offsets, 1)
		{
		}

		protected override bool Read(ref byte[] data)
		{
			return data[0] == 1;
		}

		protected override void Write(bool value, ref byte[] data)
		{
			data[0] = (byte)(value ? 1 : 0);
		}
	}
}
