// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Marshalers
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class IntMarshaler : MarshalerBase<int>
	{
		public IntMarshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 4)
		{
		}

		protected override int Read(ref byte[] data)
		{
			return BitConverter.ToInt32(data, 0);
		}

		protected override void Write(int value, ref byte[] data)
		{
			Array.Copy(BitConverter.GetBytes(value), data, 4);
		}
	}
}
