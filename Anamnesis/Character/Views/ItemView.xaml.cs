// © Anamnesis.
// Developed by W and A Walsh.
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
	using Anamnesis.Styles.DependencyProperties;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;

	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for ItemView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ItemView : UserControl
	{
		public static readonly IBind<ItemSlots> SlotDp = Binder.Register<ItemSlots, ItemView>("Slot");
		public static readonly IBind<IStructViewModel?> ItemModelDp = Binder.Register<IStructViewModel?, ItemView>(nameof(ItemModel), OnItemModelChanged, BindMode.TwoWay);
		public static readonly IBind<WeaponSubExtendedViewModel?> WeaponExModelDp = Binder.Register<WeaponSubExtendedViewModel?, ItemView>(nameof(ExtendedViewModel));

		private bool lockViewModel = false;

		public ItemView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.ContentArea.DataContext = this;
		}

		public GposeService GPoseService => GposeService.Instance;

		public ItemSlots Slot
		{
			get => SlotDp.Get(this);
			set => SlotDp.Set(this, value);
		}

		public IItem? Item { get; set; }
		public IDye? Dye { get; set; }
		public ImageSource? IconSource { get; set; }

		public IStructViewModel? ItemModel
		{
			get => ItemModelDp.Get(this);
			set => ItemModelDp.Set(this, value);
		}

		public ActorViewModel? Actor => this.DataContext as ActorViewModel;

		public WeaponSubExtendedViewModel? ExtendedViewModel
		{
			get => WeaponExModelDp.Get(this);
			set => WeaponExModelDp.Set(this, value);
		}

		public uint ItemKey
		{
			get
			{
				return this.Item?.Key ?? 0;
			}
			set
			{
				IItem? item = GameDataService.Items?.Get(value);
				this.SetItem(item, false);
			}
		}

		public string SlotName
		{
			get => LocalizationService.GetString("Character_Equipment_" + this.Slot);
		}

		public bool IsWeapon
		{
			get
			{
				return this.Slot == ItemSlots.MainHand || this.Slot == ItemSlots.OffHand;
			}
		}

		public bool IsValidWeapon
		{
			get
			{
				if (!this.IsWeapon)
					return false;

				if (this.Item == null)
					return true;

				if (this.Item.ModelSet == 0 && this.Item.SubModelSet == 0)
					return false;

				return true;
			}
		}

		private static void OnItemModelChanged(ItemView sender, IStructViewModel? value)
		{
			if (sender.ItemModel != null)
				sender.ItemModel.PropertyChanged -= sender.OnViewModelPropertyChanged;

			if (sender.ItemModel == null)
				return;

			sender.IconSource = sender.Slot.GetIcon();
			sender.ItemModel.PropertyChanged += sender.OnViewModelPropertyChanged;

			sender.OnViewModelPropertyChanged(null, null);
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
			EquipmentSelector selector = new EquipmentSelector(this.Slot);
			SelectorDrawer.Show(selector, this.Item, (i) => this.SetItem(i, selector.PairEquip));
		}

		private void SetItem(IItem? item, bool pairEquip)
		{
			if (item == this.Item)
				return;

			this.lockViewModel = true;

			IMemoryViewModel? memory = this.ItemModel?.GetParent<IMemoryViewModel>();
			memory?.SetMemoryMode(MemoryModes.Write);

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

			if (pairEquip
				&& this.ItemModel is WeaponViewModel
				&& item != null
				&& item.SubModelSet != 0
				&& this.Actor?.MainHand != null
				&& this.Actor?.OffHand != null)
			{
				this.Actor.MainHand.Set = item.ModelSet;
				this.Actor.MainHand.Base = item.ModelBase;
				this.Actor.MainHand.Variant = item.ModelVariant;

				this.Actor.OffHand.Set = item.SubModelSet;
				this.Actor.OffHand.Base = item.SubModelBase;
				this.Actor.OffHand.Variant = item.SubModelVariant;
			}
			else
			{
				if (this.ItemModel is ItemViewModel itemView)
				{
					itemView.Base = modelBase;
					itemView.Variant = (byte)modelVariant;

					if (modelBase == 0)
					{
						itemView.Dye = 0;
					}
				}
				else if (this.ItemModel is WeaponViewModel weaponView)
				{
					weaponView.Set = modelSet;
					weaponView.Base = modelBase;
					weaponView.Variant = modelVariant;

					if (modelSet == 0)
					{
						weaponView.Dye = 0;
					}
				}
			}

			this.Item = item;
			memory?.SetMemoryMode(MemoryModes.ReadWrite);
			this.lockViewModel = false;
		}

		private void OnDyeClick(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<DyeSelector, IDye>(this.Dye, (v) =>
			{
				if (v == null)
					return;

				if (this.ItemModel is ItemViewModel item)
				{
					item.Dye = v.Id;
				}
				else if (this.ItemModel is WeaponViewModel weapon)
				{
					weapon.Dye = v.Id;
				}
			});
		}

		private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs? e)
		{
			if (this.lockViewModel)
				return;

			Application.Current.Dispatcher.Invoke((Action)(() =>
			{
				if (this.ItemModel == null || !this.ItemModel.Enabled || GameDataService.Dyes == null)
					return;

				if (this.ItemModel is ItemViewModel item)
				{
					this.Item = ItemUtility.GetItem(this.Slot, 0, item.Base, item.Variant);
					this.Dye = GameDataService.Dyes.Get(item.Dye);
				}
				else if (this.ItemModel is WeaponViewModel weapon)
				{
					this.Item = ItemUtility.GetItem(this.Slot, weapon.Set, weapon.Base, weapon.Variant);

					if (weapon.Set == 0)
						weapon.Dye = 0;

					this.Dye = GameDataService.Dyes.Get(weapon.Dye);
				}
			}));
		}
	}
}
