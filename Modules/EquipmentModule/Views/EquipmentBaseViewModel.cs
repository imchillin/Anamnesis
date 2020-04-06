// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.ComponentModel;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Services;

	public abstract class EquipmentBaseViewModel : INotifyPropertyChanged
	{
		protected IGameDataService gameData;

		protected ushort modelBase;
		protected ushort modelSet;
		protected ushort modelVariant;
		protected byte dyeId;

		private IItem item;

		public EquipmentBaseViewModel(ItemSlots slot)
		{
			this.gameData = Module.Services.Get<IGameDataService>();
			this.Slot = slot;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ItemSlots Slot
		{
			get;
			private set;
		}

		public IItem Item
		{
			get
			{
				return this.item;
			}

			set
			{
				this.item = value;
				this.modelSet = value.WeaponSet;
				this.modelBase = value.ModelBase;
				this.modelVariant = value.ModelVariant;

				this.Apply();
			}
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

		public byte DyeId
		{
			get
			{
				return this.dyeId;
			}
			set
			{
				this.dyeId = value;
				this.Dye = this.GetDye(this.DyeId);
			}
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

		public bool HasModelSet
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

		public ushort ModelBase
		{
			get
			{
				return this.modelBase;
			}
			set
			{
				this.modelBase = value;
				this.Item = this.GetItem();
			}
		}

		public ushort ModelVariant
		{
			get
			{
				return this.modelVariant;
			}

			set
			{
				this.modelVariant = value;
				this.Item = this.GetItem();
			}
		}

		public ushort ModelSet
		{
			get
			{
				return this.modelSet;
			}
			set
			{
				this.modelSet = value;
				this.Item = this.GetItem();
			}
		}

		protected abstract void Apply();

		protected IItem GetItem()
		{
			foreach (IItem tItem in this.gameData.Items.All)
			{
				if (!tItem.FitsInSlot(this.Slot))
					continue;

				// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
				if (this.Slot == ItemSlots.Wrists && tItem.Name.StartsWith("Promise of"))
					continue;

				if (tItem.ModelBase == this.ModelBase && tItem.ModelVariant == this.ModelVariant && (this.HasModelSet && tItem.WeaponSet == this.ModelSet))
				{
					return tItem;
				}
			}

			return new DummyItem(this.ModelSet, this.ModelBase, this.ModelVariant);
		}

		private IDye GetDye(byte dyeId)
		{
			// None
			if (dyeId == 0)
				return null;

			foreach (IDye dye in this.gameData.Dyes.All)
			{
				if (dye.Id == dyeId)
				{
					return dye;
				}
			}

			return null;
		}

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

			public bool FitsInSlot(ItemSlots slot)
			{
				return true;
			}
		}
	}
}
