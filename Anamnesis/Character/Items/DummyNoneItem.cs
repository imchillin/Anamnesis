// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.TexTools;

	public class DummyNoneItem : IItem
	{
		public string Name => "None";
		public string? Description => null;
		public ImageSource? Icon => null;
		public ushort ModelBase => 0;
		public ushort ModelVariant => 0;
		public ushort ModelSet => 0;
		public int Key => 0;
		public bool IsWeapon => true;
		public bool HasSubModel => false;
		public ushort SubModelBase => 0;
		public ushort SubModelVariant => 0;
		public ushort SubModelSet => 0;
		public Classes EquipableClasses => Classes.All;
		public Mod? Mod => null;

		public bool FitsInSlot(ItemSlots slot)
		{
			return true;
		}
	}
}
