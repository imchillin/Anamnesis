// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System;
	using System.ComponentModel;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Services;

	public class ItemViewModel : INotifyPropertyChanged
	{
		private IGameDataService gameData;

		public ItemViewModel(ItemSlots slot)
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
			this.Item = this.gameData.Items.Get(1);
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
	}
}
