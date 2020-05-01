// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class WeaponMemory : MemoryBase<Weapon>
	{
		public WeaponMemory(IProcess process, UIntPtr address)
			: base(process, address, 7)
		{
		}

		protected override Weapon Read(ref byte[] data)
		{
			Weapon value = new Weapon();
			value.Set = BitConverter.ToUInt16(data, 0);
			value.Base = BitConverter.ToUInt16(data, 2);
			value.Variant = BitConverter.ToUInt16(data, 4);
			value.Dye = data[6];
			return value;
		}

		protected override void Write(Weapon value, ref byte[] data)
		{
			Array.Copy(BitConverter.GetBytes(value.Set), data, 2);
			Array.Copy(BitConverter.GetBytes(value.Base), 0, data, 2, 2);
			Array.Copy(BitConverter.GetBytes(value.Variant), 0, data, 4, 2);
			data[6] = value.Dye;
		}
	}
}
