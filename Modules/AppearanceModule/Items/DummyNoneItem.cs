// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Items
{
	using System;
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

		public ushort WeaponSet
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
		public ushort SubWeaponSet { get; }

		public bool FitsInSlot(ItemSlots slot)
		{
			return true;
		}
	}
}
