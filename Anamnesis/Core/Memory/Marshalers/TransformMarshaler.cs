// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Marshalers
{
	using System;
	using Anamnesis.Memory.Offsets;
	using Anamnesis.Memory.Process;

	internal class TransformMarshaler : MarshalerBase<Transform>
	{
		public TransformMarshaler(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 44)
		{
		}

		protected override Transform Read(ref byte[] data)
		{
			Transform value = default;

			Vector pos = default;
			pos.X = BitConverter.ToSingle(data, 0);
			pos.Y = BitConverter.ToSingle(data, 4);
			pos.Z = BitConverter.ToSingle(data, 8);
			////pos.W = BitConverter.ToSingle(data, 12);
			value.Position = pos;

			Quaternion rot = default;
			rot.X = BitConverter.ToSingle(data, 16);
			rot.Y = BitConverter.ToSingle(data, 20);
			rot.Z = BitConverter.ToSingle(data, 24);
			rot.W = BitConverter.ToSingle(data, 28);
			value.Rotation = rot;

			Vector scale = default;
			scale.X = BitConverter.ToSingle(data, 32);
			scale.Y = BitConverter.ToSingle(data, 36);
			scale.Z = BitConverter.ToSingle(data, 40);
			////scale.W = BitConverter.ToSingle(data, 44);
			value.Scale = scale;

			return value;
		}

		protected override void Write(Transform value, ref byte[] data)
		{
			// position
			Array.Copy(BitConverter.GetBytes(value.Position.X), 0, data, 0, 4);
			Array.Copy(BitConverter.GetBytes(value.Position.Y), 0, data, 4, 4);
			Array.Copy(BitConverter.GetBytes(value.Position.Z), 0, data, 8, 4);
			////Array.Copy(BitConverter.GetBytes(value.Position.W), 0, data, 12, 4);

			// rotation
			Array.Copy(BitConverter.GetBytes(value.Rotation.X), 0, data, 16, 4);
			Array.Copy(BitConverter.GetBytes(value.Rotation.Y), 0, data, 20, 4);
			Array.Copy(BitConverter.GetBytes(value.Rotation.Z), 0, data, 24, 4);
			Array.Copy(BitConverter.GetBytes(value.Rotation.W), 0, data, 28, 4);

			// scale
			Array.Copy(BitConverter.GetBytes(value.Scale.X), 0, data, 32, 4);
			Array.Copy(BitConverter.GetBytes(value.Scale.Y), 0, data, 36, 4);
			Array.Copy(BitConverter.GetBytes(value.Scale.Z), 0, data, 40, 4);
			////Array.Copy(BitConverter.GetBytes(value.Scale.W), 0, data, 44, 4);
		}
	}
}
