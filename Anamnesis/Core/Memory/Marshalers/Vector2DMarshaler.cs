// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;

	internal class Vector2DMarshaler : MarshalerBase<Vector2D>
	{
		public Vector2DMarshaler(params IMemoryOffset[] offsets)
			: base(offsets, 8)
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
