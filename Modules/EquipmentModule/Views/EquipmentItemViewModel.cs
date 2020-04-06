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

	public class EquipmentItemViewModel : INotifyPropertyChanged
	{
		private IGameDataService gameData;

		public EquipmentItemViewModel(Equipment target, ItemSlots slot)
			: this(slot)
		{
			Equipment.Item item = target.GetItem(slot);

			if (item == null)
				return;

			this.Item = this.GetItem(item, slot);
			this.Dye = this.GetDye(item);
		}

		public EquipmentItemViewModel(ItemSlots slot)
		{
			this.gameData = Module.Services.Get<IGameDataService>();
			this.Slot = slot;

			// hmm. doesn't seem to be any reason you _cant_ dye accessories...
			// TODO: test what happens when you dye an accessory.
			this.CanDye = slot != ItemSlots.Ears
				&& slot != ItemSlots.Neck
				&& slot != ItemSlots.Wrists
				&& slot != ItemSlots.LeftRing
				&& slot != ItemSlots.RightRing;
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
			get;
			set;
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
				return false;
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
				return false;
			}
		}

		public bool HasModelSet
		{
			get
			{
				return false;
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
				return null;
			}
			set
			{
			}
		}

		private IItem GetItem(Equipment.Item item, ItemSlots slot)
		{
			foreach (IItem tItem in this.gameData.Items.All)
			{
				if (!tItem.FitsInSlot(slot))
					continue;

				// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
				if (slot == ItemSlots.Wrists && tItem.Name.StartsWith("Promise of"))
					continue;

				if (tItem.ModelBase == item.Base && tItem.ModelVariant == item.Variant)
				{
					return tItem;
				}
			}

			return new DummyItem(item);
		}

		private IDye GetDye(Equipment.Item item)
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

		public class DummyItem : IItem
		{
			public DummyItem(Equipment.Item item)
			{
				this.ModelBase = item.Base;
				this.ModelVariant = item.Variant;
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

			public bool FitsInSlot(ItemSlots slot)
			{
				return true;
			}
		}
	}
}
