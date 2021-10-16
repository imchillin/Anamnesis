// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using Anamnesis.GameData;

	public class ItemMemory : MemoryBase
	{
		[Bind(0x000)] public ushort Base { get; set; }
		[Bind(0x002)] public byte Variant { get; set; }
		[Bind(0x003)] public byte Dye { get; set; }

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
