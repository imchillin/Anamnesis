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

	public class DummyNoneItem : IItem
	{
		public string Name
		{
			get
			{
				return "None";
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
				return 0;
			}
		}

		public ushort ModelVariant
		{
			get
			{
				return 0;
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
				return true;
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
			return true;
		}
	}
}
