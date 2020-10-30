// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.TexTools;

	public class NpcBodyItem : IItem
	{
		public string Name => "SmallClothes Body (NPC)";
		public string? Description => null;
		public ImageSource? Icon => null;
		public ushort ModelBase => 9903;
		public ushort ModelVariant => 1;
		public ushort ModelSet => 0;
		public int Key => 0;
		public bool IsWeapon => false;
		public bool HasSubModel => false;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public ushort SubModelSet => 0;
		public Classes EquipableClasses => Classes.All;
		public Mod? Mod => TexToolsService.GetMod(this.Name);

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.Body || slot == ItemSlots.Feet || slot == ItemSlots.Hands || slot == ItemSlots.Legs;
		}
	}
}
