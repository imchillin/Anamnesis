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

	public class DummyItem : IItem
	{
		public DummyItem(ushort modelSet, ushort modelBase, ushort modelVariant)
		{
			this.ModelSet = modelSet;
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

		public IImageSource Icon
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

		public ushort ModelSet
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
