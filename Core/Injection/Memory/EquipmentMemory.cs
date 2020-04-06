// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class EquipmentMemory : MemoryBase<Equipment>
	{
		public EquipmentMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override Equipment Read()
		{
			byte[] bytearray = this.ReadBytes(40);

			Equipment value = new Equipment();
			Read(0, value.Head, bytearray);
			Read(4, value.Chest, bytearray);
			Read(8, value.Arms, bytearray);
			Read(12, value.Legs, bytearray);
			Read(16, value.Feet, bytearray);
			Read(20, value.Ear, bytearray);
			Read(24, value.Neck, bytearray);
			Read(28, value.Wrist, bytearray);
			Read(32, value.RFinger, bytearray);
			Read(36, value.LFinger, bytearray);
			return value;
		}

		protected override void Write(Equipment value)
		{
			byte[] bytearray = new byte[40];
			Write(0, value.Head, bytearray);
			Write(4, value.Chest, bytearray);
			Write(8, value.Arms, bytearray);
			Write(12, value.Legs, bytearray);
			Write(16, value.Feet, bytearray);
			Write(20, value.Ear, bytearray);
			Write(24, value.Neck, bytearray);
			Write(28, value.Wrist, bytearray);
			Write(32, value.RFinger, bytearray);
			Write(36, value.LFinger, bytearray);
			this.WriteBytes(bytearray);
		}

		private static void Write(int index, Equipment.Item item, byte[] data)
		{
			Array.Copy(BitConverter.GetBytes(item.Base), 0, data, index, 2);
			Array.Copy(BitConverter.GetBytes(item.Variant), 0, data, index + 2, 1);
			Array.Copy(BitConverter.GetBytes(item.Dye), 0, data, index + 3, 1);
		}

		private static void Read(int index, Equipment.Item item, byte[] data)
		{
			item.Base = BitConverter.ToUInt16(data, index);
			item.Variant = data[index + 2];
			item.Dye = data[index + 3];
		}
	}
}
