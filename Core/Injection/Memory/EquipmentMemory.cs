// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class EquipmentMemory : MemoryBase<Equipment>
	{
		public EquipmentMemory(IProcess process, IMemoryOffset[] offsets)
			: base(process, offsets, 40)
		{
		}

		protected override Equipment Read(ref byte[] data)
		{
			Equipment value = new Equipment();
			Read(0, value.Head, data);
			Read(4, value.Chest, data);
			Read(8, value.Arms, data);
			Read(12, value.Legs, data);
			Read(16, value.Feet, data);
			Read(20, value.Ear, data);
			Read(24, value.Neck, data);
			Read(28, value.Wrist, data);
			Read(32, value.RFinger, data);
			Read(36, value.LFinger, data);
			return value;
		}

		protected override void Write(Equipment value, ref byte[] data)
		{
			Write(0, value.Head, data);
			Write(4, value.Chest, data);
			Write(8, value.Arms, data);
			Write(12, value.Legs, data);
			Write(16, value.Feet, data);
			Write(20, value.Ear, data);
			Write(24, value.Neck, data);
			Write(28, value.Wrist, data);
			Write(32, value.RFinger, data);
			Write(36, value.LFinger, data);
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
