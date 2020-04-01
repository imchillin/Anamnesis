// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Windows.Media.Media3D;

	public class QuaternionMemory : MemoryBase<Quaternion>
	{
		public QuaternionMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override Quaternion Read()
		{
			byte[] bytearray = this.ReadBytes(16);

			Quaternion value = default(Quaternion);
			value.X = BitConverter.ToSingle(bytearray, 0);
			value.Y = BitConverter.ToSingle(bytearray, 4);
			value.Z = BitConverter.ToSingle(bytearray, 8);
			value.W = BitConverter.ToSingle(bytearray, 12);
			return value;
		}

		protected override void Write(Quaternion value)
		{
			byte[] bytearray = new byte[16];
			Array.Copy(BitConverter.GetBytes((float)value.X), bytearray, 4);
			Array.Copy(BitConverter.GetBytes((float)value.Y), 0, bytearray, 4, 4);
			Array.Copy(BitConverter.GetBytes((float)value.Z), 0, bytearray, 8, 4);
			Array.Copy(BitConverter.GetBytes((float)value.W), 0, bytearray, 12, 4);
			this.WriteBytes(bytearray);
		}
	}
}
