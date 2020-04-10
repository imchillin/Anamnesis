// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.ComponentModel;
	using System.Windows.Media.Media3D;
	using ConceptMatrix;
	using ConceptMatrix.Services;
	using PropertyChanged;

	public abstract class EquipmentBaseViewModel : INotifyPropertyChanged, IDisposable
	{
		public static readonly DummyNoneItem NoneItem = new DummyNoneItem();
		public static readonly DummyNoneDye NoneDye = new DummyNoneDye();

		protected IGameDataService gameData;

		protected ushort modelBase;
		protected ushort modelSet;
		protected ushort modelVariant;
		protected byte dyeId;

		private IItem item;
		private IDye dye;
		private Selection target;

		public EquipmentBaseViewModel(ItemSlots slot, Selection selection)
		{
			this.target = selection;
			this.gameData = Module.Services.Get<IGameDataService>();
			this.Slot = slot;
		}

		public delegate void ChangedHandler();

		public event PropertyChangedEventHandler PropertyChanged;
		public event ChangedHandler Changed;

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
				IItem oldItem = this.item;
				this.item = value;

				if (value != null)
				{
					this.modelSet = value.WeaponSet;
					this.modelBase = value.ModelBase;
					this.modelVariant = value.ModelVariant;
				}
				else
				{
					this.modelSet = 0;
					this.modelBase = 0;
					this.modelVariant = 0;
				}

				if (oldItem != null && oldItem != this.item)
				{
					this.Apply();
					this.target.ActorRefresh();
				}
			}
		}

		public IDye Dye
		{
			get
			{
				return this.dye;
			}

			set
			{
				IDye oldDye = this.dye;
				this.dye = value;
				this.dyeId = this.dye != null ? value.Id : (byte)0;

				if (oldDye != null && oldDye != this.dye)
				{
					this.Apply();
					this.target.ActorRefresh();
				}
			}
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
				this.Dye = this.GetDye();
			}
		}

		public virtual Color Color
		{
			get;
			set;
		}

		public bool CanColor
		{
			get;
			set;
		}

		public virtual Vector Scale
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

		[DependsOn(nameof(EquipmentBaseViewModel.Item))]
		public int Key
		{
			get
			{
				if (this.item == null)
					return 0;

				return this.Item.Key;
			}
			set
			{
				IItem item = this.gameData.Items.Get(value);

				if (item != null && item.FitsInSlot(this.Slot))
				{
					this.Item = item;
				}
			}
		}

		[DependsOn(nameof(EquipmentBaseViewModel.Item))]
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

		[DependsOn(nameof(EquipmentBaseViewModel.Item))]
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

		[DependsOn(nameof(EquipmentBaseViewModel.Item))]
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

		public abstract void Dispose();

		protected abstract void Apply();

		protected IItem GetItem()
		{
			if (this.ModelBase == 0 && this.modelVariant == 0 && this.modelSet == 0)
				return NoneItem;

			foreach (IItem tItem in this.gameData.Items.All)
			{
				if (!tItem.FitsInSlot(this.Slot))
					continue;

				// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
				if (this.Slot == ItemSlots.Wrists && tItem.Name.StartsWith("Promise of"))
					continue;

				if (this.HasModelSet && tItem.WeaponSet != this.ModelSet)
					continue;

				if (tItem.ModelBase == this.ModelBase && tItem.ModelVariant == this.ModelVariant)
				{
					return tItem;
				}
			}

			return new DummyItem(this.ModelSet, this.ModelBase, this.ModelVariant);
		}

		protected IDye GetDye()
		{
			// None
			if (this.DyeId == 0)
				return NoneDye;

			foreach (IDye dye in this.gameData.Dyes.All)
			{
				if (dye.Id == this.DyeId)
				{
					return dye;
				}
			}

			return NoneDye;
		}

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

			public bool FitsInSlot(ItemSlots slot)
			{
				return true;
			}
		}

		public class DummyNoneDye : IDye
		{
			public byte Id
			{
				get
				{
					return 0;
				}
			}

			public string Name
			{
				get
				{
					return "None";
				}
			}

			public string Description { get; }
			public IImage Icon { get; }

			public int Key
			{
				get
				{
					return 0;
				}
			}
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
