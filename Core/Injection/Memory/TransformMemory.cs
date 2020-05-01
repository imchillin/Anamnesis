// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class TransformMemory : MemoryBase<Transform>
	{
		public TransformMemory(IProcess process, UIntPtr address)
			: base(process, address, 44)
		{
		}

		protected override Transform Read(ref byte[] data)
		{
			Transform value = default;

			Vector pos = default;
			pos.X = BitConverter.ToSingle(data, 0);
			pos.Y = BitConverter.ToSingle(data, 4);
			pos.Z = BitConverter.ToSingle(data, 8);
			value.Position = pos;

			value.MysteryFloat = BitConverter.ToSingle(data, 12);

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
			value.Scale = scale;

			return value;
		}

		protected override void Write(Transform value, ref byte[] data)
		{
			// position
			Array.Copy(BitConverter.GetBytes(value.Position.X), 0, data, 0, 4);
			Array.Copy(BitConverter.GetBytes(value.Position.Y), 0, data, 4, 4);
			Array.Copy(BitConverter.GetBytes(value.Position.Z), 0, data, 8, 4);

			// mystery
			Array.Copy(BitConverter.GetBytes(value.MysteryFloat), 0, data, 12, 4);

			// rotation
			Array.Copy(BitConverter.GetBytes(value.Rotation.X), 0, data, 16, 4);
			Array.Copy(BitConverter.GetBytes(value.Rotation.Y), 0, data, 20, 4);
			Array.Copy(BitConverter.GetBytes(value.Rotation.Z), 0, data, 24, 4);
			Array.Copy(BitConverter.GetBytes(value.Rotation.W), 0, data, 28, 4);

			// scale
			Array.Copy(BitConverter.GetBytes(value.Scale.X), 0, data, 32, 4);
			Array.Copy(BitConverter.GetBytes(value.Scale.Y), 0, data, 36, 4);
			Array.Copy(BitConverter.GetBytes(value.Scale.Z), 0, data, 40, 4);
		}
	}
}
