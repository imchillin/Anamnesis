// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Marshalers
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class Color4Marshaler : MarshalerBase<Color4>
	{
		public Color4Marshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 16)
		{
		}

		protected override Color4 Read(ref byte[] data)
		{
			Color4 value = default;
			value.R = BitConverter.ToSingle(data, 0);
			value.G = BitConverter.ToSingle(data, 4);
			value.B = BitConverter.ToSingle(data, 8);
			value.A = BitConverter.ToSingle(data, 12);
			return value;
		}

		protected override void Write(Color4 value, ref byte[] data)
		{
			Array.Copy(BitConverter.GetBytes(value.R), 0, data, 0, 4);
			Array.Copy(BitConverter.GetBytes(value.G), 0, data, 4, 4);
			Array.Copy(BitConverter.GetBytes(value.B), 0, data, 8, 4);
			Array.Copy(BitConverter.GetBytes(value.A), 0, data, 12, 4);
		}
	}
}
