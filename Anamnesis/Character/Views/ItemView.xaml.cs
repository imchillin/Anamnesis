// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Excel;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;
	using Serilog;
	using XivToolsWpf;
	using XivToolsWpf.DependencyProperties;

	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for ItemView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ItemView : UserControl
	{
		public static readonly IBind<ItemSlots> SlotDp = Binder.Register<ItemSlots, ItemView>("Slot");
		public static readonly IBind<IEquipmentItemMemory?> ItemModelDp = Binder.Register<IEquipmentItemMemory?, ItemView>(nameof(ItemModel), OnItemModelChanged, BindMode.TwoWay);
		public static readonly IBind<WeaponSubModelMemory?> WeaponExModelDp = Binder.Register<WeaponSubModelMemory?, ItemView>(nameof(ExtendedViewModel));

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

		public IItem? Item { get; set; }
		public IDye? Dye { get; set; }
		public ImageSource? IconSource { get; set; }
		public bool CanDye { get; set; }
		public bool IsLoading { get; set; }

		public IEquipmentItemMemory? ItemModel
		{
			get => ItemModelDp.Get(this);
			set => ItemModelDp.Set(this, value);
		}

		public ActorMemory? Actor { get; private set; }

		public WeaponSubModelMemory? ExtendedViewModel
		{
			get => WeaponExModelDp.Get(this);
			set => WeaponExModelDp.Set(this, value);
		}

		public uint ItemKey
		{
			get
			{
				return this.Item?.RowId ?? 0;
			}
			set
			{
				IItem? item = GameDataService.Items?.Get(value);
				this.SetItem(item);
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

		public bool IsHead
		{
			get
			{
				return this.Slot == ItemSlots.Head;
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

		private static void OnItemModelChanged(ItemView sender, IEquipmentItemMemory? value)
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
			if (this.Actor?.CanRefresh != true)
				return;

			EquipmentSelector selector = new EquipmentSelector(this.Slot, this.Actor);
			SelectorDrawer.Show(selector, this.Item, (i) => this.SetItem(i, selector.AutoOffhand, selector.ForceMainModel, selector.ForceOffModel));
		}

		private void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (this.Actor?.CanRefresh != true)
				return;

			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
			{
				this.ItemModel?.Clear(this.Actor.IsPlayer);
			}
		}

		private void SetItem(IItem? item, bool autoOffhand = false, bool forceMain = false, bool forceOff = false)
		{
			this.lockViewModel = true;

			if (item != null)
			{
				bool useSubModel = this.Slot == ItemSlots.OffHand && item.HasSubModel;

				if (item.HasSubModel)
				{
					if (forceMain)
					{
						useSubModel = false;
					}
					else if (forceOff)
					{
						useSubModel = true;
					}
				}

				ushort modelSet = useSubModel ? item.SubModelSet : item.ModelSet;
				ushort modelBase = useSubModel ? item.SubModelBase : item.ModelBase;
				ushort modelVariant = useSubModel ? item.SubModelVariant : item.ModelVariant;

				this.SetModel(this.ItemModel, modelSet, modelBase, modelVariant);

				if (autoOffhand && this.Slot == ItemSlots.MainHand
					&& item is Item ivm
					&& ivm.EquipSlot?.OffHand == -1)
				{
					if (ivm.HasSubModel)
					{
						this.SetModel(this.Actor?.OffHand, ivm.SubModelSet, ivm.SubModelBase, ivm.SubModelVariant);
					}
					else
					{
						this.SetModel(this.Actor?.OffHand, 0, 0, 0);
					}
				}

				if (item == ItemUtility.NoneItem || item == ItemUtility.EmperorsNewFists)
				{
					this.Dye = ItemUtility.NoneDye;
				}
			}

			this.Item = item;
			this.lockViewModel = false;
		}

		private void SetModel(IEquipmentItemMemory? itemModel, ushort modelSet, ushort modelBase, ushort modelVariant)
		{
			if (itemModel is ItemMemory itemView)
			{
				itemView.Base = modelBase;
				itemView.Variant = (byte)modelVariant;

				if (modelBase == 0)
				{
					itemView.Dye = 0;
				}
			}
			else if (itemModel is WeaponMemory weaponView)
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

		private void OnDyeClick(object sender, RoutedEventArgs e)
		{
			if (!this.CanDye)
				return;

			SelectorDrawer.Show<DyeSelector, IDye>(this.Dye, (v) =>
			{
				if (v == null)
					return;

				if (this.ItemModel is ItemMemory item)
				{
					item.Dye = v.Id;
				}
				else if (this.ItemModel is WeaponMemory weapon)
				{
					weapon.Dye = v.Id;
				}
			});
		}

		private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs? e)
		{
			if (this.lockViewModel)
				return;

			Task.Run(async () =>
			{
				await Task.Yield();
				await Dispatch.MainThread();
				if (this.ItemModel == null || GameDataService.Dyes == null)
					return;

				this.IsLoading = true;

				try
				{
					IEquipmentItemMemory? valueVm = this.ItemModel;
					ItemSlots slots = this.Slot;

					await Dispatch.NonUiThread();

					if (valueVm is ItemMemory itemVm)
					{
						IItem? item = ItemUtility.GetItem(slots, 0, itemVm.Base, itemVm.Variant);
						IDye? dye = GameDataService.Dyes.Get(itemVm.Dye);

						await Dispatch.MainThread();

						this.Item = item;
						this.Dye = dye;
					}
					else if (valueVm is WeaponMemory weaponVm)
					{
						IItem? item = ItemUtility.GetItem(slots, weaponVm.Set, weaponVm.Base, weaponVm.Variant);

						if (weaponVm.Set == 0)
							weaponVm.Dye = 0;

						IDye? dye = GameDataService.Dyes.Get(weaponVm.Dye);

						await Dispatch.MainThread();

						this.Item = item;
						this.Dye = dye;
					}

					this.CanDye = !this.IsWeapon || this.ItemModel?.Set != 0;
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed to update item");
				}

				this.IsLoading = false;
			});
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.Actor = this.DataContext as ActorMemory;
		}
	}
}
