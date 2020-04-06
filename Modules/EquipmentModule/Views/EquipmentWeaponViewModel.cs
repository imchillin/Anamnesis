// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System;
	using System.ComponentModel;
	using System.Windows.Media.Media3D;
	using ConceptMatrix;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Services;

	public class EquipmentWeaponViewModel : INotifyPropertyChanged
	{
		private IGameDataService gameData;

		public EquipmentWeaponViewModel(Weapon weapon, ItemSlots slot)
		{
			this.gameData = Module.Services.Get<IGameDataService>();
			this.Slot = slot;
			this.Color = new Color();
			this.Scale = new Vector3D(1, 1, 1);

			this.Item = this.GetItem(weapon, slot);
			this.Dye = this.GetDye(weapon);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ItemSlots Slot
		{
			get;
			private set;
		}

		public IItem Item
		{
			get;
			set;
		}

		public IDye Dye
		{
			get;
			set;
		}

		public bool CanDye
		{
			get
			{
				return true;
			}
		}

		public Color Color
		{
			get;
			set;
		}

		public bool CanColor
		{
			get
			{
				return true;
			}
		}

		public Vector3D Scale
		{
			get;
			set;
		}

		public bool CanScale
		{
			get
			{
				return true;
			}
		}

		public bool HasModelSet
		{
			get
			{
				return true;
			}
		}

		public string SlotName
		{
			get
			{
				return this.Slot.ToDisplayName();
			}
		}

		public string Key
		{
			get
			{
				return this.Item?.Key.ToString();
			}
			set
			{
				int val = int.Parse(value);
				this.Item = this.gameData.Items.Get(val);
			}
		}

		public string ModelBase
		{
			get
			{
				return this.Item?.ModelBase.ToString();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string ModelVariant
		{
			get
			{
				return this.Item?.ModelVariant.ToString();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public string ModelSet
		{
			get
			{
				return this.Item?.WeaponSet.ToString();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		private IItem GetItem(Weapon weapon, ItemSlots slot)
		{
			foreach (IItem tItem in this.gameData.Items.All)
			{
				if (!tItem.FitsInSlot(slot))
					continue;

				if (tItem.ModelBase == weapon.Base && tItem.ModelVariant == weapon.Variant && tItem.WeaponSet == weapon.Set)
				{
					return tItem;
				}
			}

			return new DummyWeapon(weapon);
		}

		private IDye GetDye(Weapon item)
		{
			// None
			if (item.Dye == 0)
				return null;

			foreach (IDye dye in this.gameData.Dyes.All)
			{
				if (dye.Id == item.Dye)
				{
					return dye;
				}
			}

			return null;
		}

		public class DummyWeapon : IItem
		{
			public DummyWeapon(Weapon weapon)
			{
				this.ModelBase = weapon.Base;
				this.ModelVariant = weapon.Variant;
				this.WeaponSet = weapon.Set;
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

			public bool FitsInSlot(ItemSlots slot)
			{
				return true;
			}
		}
	}
}
