// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Items
{
	using System;
	using System.Collections.Generic;
	using Anamnesis;
	using ConceptMatrix;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	public class NpcBodyItem : IItem
	{
		public string Name
		{
			get
			{
				return "NPC Body";
			}
		}

		public string Description
		{
			get
			{
				return null;
			}
		}

		public IImage Icon
		{
			get
			{
				return null;
			}
		}

		public ushort ModelBase
		{
			get
			{
				return 9903;
			}
		}

		public ushort ModelVariant
		{
			get
			{
				return 1;
			}
		}

		public ushort ModelSet
		{
			get
			{
				return 0;
			}
		}

		public int Key
		{
			get
			{
				return 0;
			}
		}

		public bool IsWeapon
		{
			get
			{
				return false;
			}
		}

		public bool HasSubModel
		{
			get
			{
				return false;
			}
		}

		public ushort SubModelBase { get; }
		public ushort SubModelVariant { get; }
		public ushort SubModelSet { get; }

		public Classes EquipableClasses
		{
			get
			{
				return Classes.All;
			}
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.Body || slot == ItemSlots.Feet || slot == ItemSlots.Hands || slot == ItemSlots.Legs;
		}
	}
}
