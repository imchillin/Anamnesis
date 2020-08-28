// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Marshalers
{
	using System;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	internal class Vector2DMarshaler : MarshalerBase<Vector2D>
	{
		public Vector2DMarshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 8)
		{
		}

		protected override Vector2D Read(ref byte[] data)
		{
			Vector2D value = default;
			value.X = BitConverter.ToSingle(data, 0);
			value.Y = BitConverter.ToSingle(data, 4);
			return value;
		}

		protected override void Write(Vector2D value, ref byte[] data)
		{
			Array.Copy(BitConverter.GetBytes(value.X), 0, data, 0, 4);
			Array.Copy(BitConverter.GetBytes(value.Y), 0, data, 4, 4);
		}
	}
}
