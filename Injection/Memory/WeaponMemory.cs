// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class WeaponMemory : MemoryBase<Weapon>
	{
		public WeaponMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override Weapon Read()
		{
			byte[] bytearray = this.ReadBytes(7);

			Weapon value = new Weapon();
			value.Set = BitConverter.ToUInt16(bytearray, 0);
			value.Base = BitConverter.ToUInt16(bytearray, 2);
			value.Variant = BitConverter.ToUInt16(bytearray, 4);
			value.Dye = bytearray[6];
			return value;
		}

		protected override void Write(Weapon value)
		{
			byte[] bytearray = new byte[7];
			Array.Copy(BitConverter.GetBytes(value.Set), bytearray, 2);
			Array.Copy(BitConverter.GetBytes(value.Base), 0, bytearray, 2, 2);
			Array.Copy(BitConverter.GetBytes(value.Variant), 0, bytearray, 4, 2);
			bytearray[6] = value.Dye;
			this.WriteBytes(bytearray);
		}
	}
}
