// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using Anamnesis.AppearanceModule.Utilities;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.WpfStyles.DependencyProperties;
	using Anamnesis.WpfStyles.Drawers;
	using PropertyChanged;

	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for ItemView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ItemView : UserControl
	{
		public static readonly IBind<ItemSlots> SlotDp = Binder.Register<ItemSlots, ItemView>("Slot");

		public ItemView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.ContentArea.DataContext = this;
		}

		public ItemSlots Slot
		{
			get => SlotDp.Get(this);
			set => SlotDp.Set(this, value);
		}

		public IStructViewModel ViewModel { get; set; }
		public IItem Item { get; set; }
		public IDye Dye { get; set; }
		public ImageSource IconSource { get; set; }

		public int? ItemKey
		{
			get
			{
				return this.Item?.Key;
			}
			set
			{
				IItem item = null;

				if (value != null)
					item = GameDataService.Items.Get((int)value);

				this.Item = item;

				ushort modelSet = 0;
				ushort modelBase = 0;
				ushort modelVariant = 0;

				if (value != null && item != null)
				{
					bool useSubModel = this.Slot == ItemSlots.OffHand && this.Item.HasSubModel;

					modelSet = useSubModel ? this.Item.SubModelSet : this.Item.ModelSet;
					modelBase = useSubModel ? this.Item.SubModelBase : this.Item.ModelBase;
					modelVariant = useSubModel ? (byte)this.Item.SubModelVariant : (byte)this.Item.ModelVariant;
				}

				if (this.ViewModel is ItemViewModel itemView)
				{
					itemView.Base = modelBase;
					itemView.Variant = (byte)modelVariant;
				}
				else if (this.ViewModel is WeaponViewModel weaponView)
				{
					weaponView.Set = modelSet;
					weaponView.Base = modelBase;
					weaponView.Variant = modelVariant;
				}
			}
		}

		public string SlotName
		{
			get => this.Slot.ToDisplayName();
		}

		public bool IsWeapon
		{
			get
			{
				return this.Slot == ItemSlots.MainHand || this.Slot == ItemSlots.OffHand;
			}
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
			EquipmentSelector selector = new EquipmentSelector(this.Slot);
			SelectorDrawer.Show<IItem>("Select " + this.SlotName, selector, this.Item, (v) => { this.ItemKey = v.Key; });
		}

		private void OnDyeClick(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<DyeSelector, IDye>("Select Dye", this.Dye, (v) =>
			{
				if (this.ViewModel is ItemViewModel item)
				{
					item.Dye = (byte)v.Key;
				}
				else if (this.ViewModel is WeaponViewModel weapon)
				{
					weapon.Dye = (byte)v.Key;
				}
			});
		}

		private void OnThisDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.ViewModel = this.DataContext as IStructViewModel;

			if (this.ViewModel == null)
				return;

			this.IconSource = this.Slot.GetIcon();
			this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
			this.OnViewModelPropertyChanged(null, null);
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!this.ViewModel.Enabled)
				return;

			Application.Current.Dispatcher.Invoke(() =>
			{
				if (this.ViewModel is ItemViewModel item)
				{
					this.Item = ItemUtility.GetItem(this.Slot, 0, item.Base, item.Variant);
					this.Dye = GameDataService.Dyes.Get(item.Dye);
				}
				else if (this.ViewModel is WeaponViewModel weapon)
				{
					this.Item = ItemUtility.GetItem(this.Slot, weapon.Set, weapon.Base, weapon.Variant);
					this.Dye = GameDataService.Dyes.Get(weapon.Dye);
				}
			});
		}
	}
}
