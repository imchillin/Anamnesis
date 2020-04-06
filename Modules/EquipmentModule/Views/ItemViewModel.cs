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

	public class ItemViewModel : INotifyPropertyChanged
	{
		private IGameDataService gameData;

		public ItemViewModel(Equipment target, ItemSlots slot)
		{
			this.gameData = Module.Services.Get<IGameDataService>();
			this.Slot = slot;

			this.HasModelId = slot == ItemSlots.MainHand || slot == ItemSlots.OffHand;
			this.CanScale = slot == ItemSlots.MainHand || slot == ItemSlots.OffHand;
			this.CanColor = slot == ItemSlots.MainHand || slot == ItemSlots.OffHand;

			this.CanDye = slot != ItemSlots.Ears
				&& slot != ItemSlots.Neck
				&& slot != ItemSlots.Wrists
				&& slot != ItemSlots.LeftRing
				&& slot != ItemSlots.RightRing;

			this.Color = new Color();
			this.Scale = new Vector3D(1, 1, 1);
			////this.Item = this.gameData.Items.Get(1);

			Equipment.Item item = target.GetItem(slot);

			if (item == null)
				return;

			this.Item = this.GetItem(item, slot);
			this.Dye = this.GetDye(item);
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
			get;
			set;
		}

		public Vector3D Scale
		{
			get;
			set;
		}

		public bool CanScale
		{
			get;
			set;
		}

		public bool HasModelId
		{
			get;
			set;
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

		public string ModelBaseId
		{
			get
			{
				return this.Item?.ModelBaseId.ToString();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string ModelVariantId
		{
			get
			{
				return this.Item?.ModelVariantId.ToString();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public string ModelId
		{
			get
			{
				return this.Item?.ModelId.ToString();
			}

			set
			{
				throw new NotImplementedException();
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

				if (tItem.ModelBaseId == item.Base && tItem.ModelVariantId == item.Varaint)
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
				this.ModelBaseId = item.Base;
				this.ModelVariantId = item.Varaint;
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

			public ushort ModelBaseId
			{
				get;
				private set;
			}

			public byte ModelVariantId
			{
				get;
				private set;
			}

			public byte ModelId
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
