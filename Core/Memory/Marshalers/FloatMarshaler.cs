// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Marshalers
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class FloatMarshaler : MarshalerBase<float>
	{
		public FloatMarshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 4)
		{
		}

		protected override float Read(ref byte[] data)
		{
			return BitConverter.ToSingle(data, 0);
		}

		protected override void Write(float value, ref byte[] data)
		{
			Array.Copy(BitConverter.GetBytes(value), data, 4);
		}
	}
}
