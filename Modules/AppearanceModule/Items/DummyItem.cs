// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Items
{
	using System;
	using Anamnesis;
	using ConceptMatrix;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	public class DummyItem : IItem
	{
		public DummyItem(ushort modelSet, ushort modelBase, ushort modelVariant)
		{
			this.WeaponSet = modelSet;
			this.ModelBase = modelBase;
			this.ModelVariant = modelVariant;
		}

		public string Name
		{
			get
			{
				return "Unknown";
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
			get;
			private set;
		}

		public ushort ModelVariant
		{
			get;
			private set;
		}

		public ushort WeaponSet
		{
			get;
			private set;
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

		public bool FitsInSlot(ItemSlots slot)
		{
			return true;
		}
	}
}
