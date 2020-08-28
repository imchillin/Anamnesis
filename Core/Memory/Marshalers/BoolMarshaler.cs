// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Marshalers
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class BoolMarshaler : MarshalerBase<bool>
	{
		public BoolMarshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 1)
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
