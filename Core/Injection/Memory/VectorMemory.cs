// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class VectorMemory : MemoryBase<Vector>
	{
		public VectorMemory(ProcessInjection process, UIntPtr address)
			: base(process, address, 12)
		{
		}

		protected override Vector Read(ref byte[] data)
		{
			Vector value = default(Vector);
			value.X = BitConverter.ToSingle(data, 0);
			value.Y = BitConverter.ToSingle(data, 4);
			value.Z = BitConverter.ToSingle(data, 8);
			return value;
		}

		protected override void Write(Vector value, ref byte[] data)
		{
			Array.Copy(BitConverter.GetBytes(value.X), data, 4);
			Array.Copy(BitConverter.GetBytes(value.Y), 0, data, 4, 4);
			Array.Copy(BitConverter.GetBytes(value.Z), 0, data, 8, 4);
		}
	}
}
