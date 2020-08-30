// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;

	internal class FloatMarshaler : MarshalerBase<float>
	{
		public FloatMarshaler(params IMemoryOffset[] offsets)
			: base(offsets, 4)
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
