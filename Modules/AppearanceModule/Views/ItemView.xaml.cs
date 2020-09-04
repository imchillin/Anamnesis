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

		public ItemViewModel ViewModel { get; set; }
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

				if (value == null || item == null)
				{
					this.ViewModel.Base = 0;
					this.ViewModel.Variant = 0;
				}
				else
				{
					bool useSubModel = this.Slot == ItemSlots.OffHand && this.Item.HasSubModel;

					////this.ViewModel.Set = useSubModel ? this.Item.SubModelSet : this.Item.ModelSet;
					this.ViewModel.Base = useSubModel ? this.Item.SubModelBase : this.Item.ModelBase;
					this.ViewModel.Variant = useSubModel ? (byte)this.Item.SubModelVariant : (byte)this.Item.ModelVariant;
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
			SelectorDrawer.Show<DyeSelector, IDye>("Select Dye", this.Dye, (v) => { this.ViewModel.Dye = (byte)v.Key; });
		}

		private void OnThisDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.ViewModel = this.DataContext as ItemViewModel;

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
				this.Item = ItemUtility.GetItem(this.Slot, 0, this.ViewModel.Base, this.ViewModel.Variant);
				this.Dye = GameDataService.Dyes.Get(this.ViewModel.Dye);
			});
		}
	}
}
