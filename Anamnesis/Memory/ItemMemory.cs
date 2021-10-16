// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using Anamnesis.GameData;

	public class ItemMemory : MemoryBase, IEquipmentItemMemory
	{
		[Bind(0x000, BindFlags.ActorRefresh)] public ushort Base { get; set; }
		[Bind(0x002, BindFlags.ActorRefresh)] public byte Variant { get; set; }
		[Bind(0x003, BindFlags.ActorRefresh)] public byte Dye { get; set; }

		public void Clear()
		{
			this.Base = 0;
			this.Variant = 0;
			this.Dye = 0;
		}

		public void Equip(IItem item)
		{
			this.Base = item.ModelBase;
			this.Variant = (byte)item.ModelVariant;
		}
	}
}
