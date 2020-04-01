// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Windows.Media.Media3D;

	public class Vector3DMemory : MemoryBase<Vector3D>
	{
		public Vector3DMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override Vector3D Read()
		{
			byte[] bytearray = this.ReadBytes(12);

			Vector3D value = default(Vector3D);
			value.X = BitConverter.ToSingle(bytearray, 0);
			value.Y = BitConverter.ToSingle(bytearray, 4);
			value.Z = BitConverter.ToSingle(bytearray, 8);
			return value;
		}

		protected override void Write(Vector3D value)
		{
			byte[] bytearray = new byte[16];
			Array.Copy(BitConverter.GetBytes((float)value.X), bytearray, 4);
			Array.Copy(BitConverter.GetBytes((float)value.Y), 0, bytearray, 4, 4);
			Array.Copy(BitConverter.GetBytes((float)value.Z), 0, bytearray, 8, 4);
			this.WriteBytes(bytearray);
		}
	}
}
