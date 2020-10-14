// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using Anamnesis.Character.Utilities;
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
	[SuppressPropertyChangedWarnings]
	public partial class ItemView : UserControl
	{
		public static readonly IBind<ItemSlots> SlotDp = Binder.Register<ItemSlots, ItemView>("Slot");

		private bool lockViewModel = false;

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

		public IStructViewModel? ViewModel { get; set; }
		public IItem? Item { get; set; }
		public IDye? Dye { get; set; }
		public ImageSource? IconSource { get; set; }

		public int ItemKey
		{
			get
			{
				return this.Item?.Key ?? 0;
			}
			set
			{
				IItem? item = GameDataService.Items?.Get((int)value);
				this.SetItem(item);
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
			SelectorDrawer.Show("Select " + this.SlotName, selector, this.Item, this.SetItem);
		}

		private void SetItem(IItem? item)
		{
			this.lockViewModel = true;

			ushort modelSet = 0;
			ushort modelBase = 0;
			ushort modelVariant = 0;

			if (item != null)
			{
				bool useSubModel = this.Slot == ItemSlots.OffHand && item.HasSubModel;

				modelSet = useSubModel ? item.SubModelSet : item.ModelSet;
				modelBase = useSubModel ? item.SubModelBase : item.ModelBase;
				modelVariant = useSubModel ? (byte)item.SubModelVariant : (byte)item.ModelVariant;
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

			this.Item = item;
			this.lockViewModel = false;
		}

		private void OnDyeClick(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<DyeSelector, IDye>("Select Dye", this.Dye, (v) =>
			{
				if (v == null)
					return;

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
			if (this.ViewModel != null)
				this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;

			this.ViewModel = this.DataContext as IStructViewModel;

			if (this.ViewModel == null)
				return;

			this.IconSource = this.Slot.GetIcon();
			this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;

			this.OnViewModelPropertyChanged(null, null);
		}

		private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs? e)
		{
			if (this.lockViewModel)
				return;

			if (this.ViewModel == null || !this.ViewModel.Enabled || GameDataService.Dyes == null)
				return;

			Application.Current.Dispatcher.Invoke(async () =>
			{
				if (this.ViewModel is ItemViewModel item)
				{
					this.Item = await ItemUtility.GetItemAsync(this.Slot, 0, item.Base, item.Variant);
					this.Dye = GameDataService.Dyes.Get(item.Dye);
				}
				else if (this.ViewModel is WeaponViewModel weapon)
				{
					this.Item = await ItemUtility.GetItemAsync(this.Slot, weapon.Set, weapon.Base, weapon.Variant);
					this.Dye = GameDataService.Dyes.Get(weapon.Dye);
				}
			});
		}
	}
}
