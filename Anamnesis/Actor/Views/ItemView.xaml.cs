// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Utilities;
using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using Anamnesis.Utils;
using PropertyChanged;
using Serilog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for ItemView.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class ItemView : UserControl
{
	public static readonly IBind<ItemSlots> SlotDp = Binder.Register<ItemSlots, ItemView>(nameof(Slot));
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
	public IDye? Dye2 { get; set; }
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

	public string SlotName => LocalizationService.GetString("Character_Equipment_" + this.Slot);

	public bool IsWeapon => (this.Slot & ItemSlots.Weapons) != 0;

	public bool IsHead => this.Slot == ItemSlots.Head;

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

	public bool IsValidItemForFanSite
	{
		get
		{
			// Invalid if Item is null.
			if (this.Item == null)
				return false;

			// Invalid if we're naked in some way other than Emperor's.
			ushort[] invalidItems = { 0, 9901, 9903 };
			if (invalidItems.Contains(this.Item.ModelBase))
				return false;

			// Invalid if row Id is 0, which would be the case if we have a
			// set/subset combo which doesn't match an actual item.
			if (this.Item.RowId == 0)
				return false;

			// Most items will have the None category. Shop items will be premium, and old items will be deprecated.
			// If we aren't one of these, then invalid. CustomEquipment is for things like Forum Attire.
			bool isNormalCategory = this.Item.Category.HasFlagUnsafe(ItemCategories.None) ||
									this.Item.Category.HasFlagUnsafe(ItemCategories.Standard) ||
									this.Item.Category.HasFlagUnsafe(ItemCategories.Premium) ||
									this.Item.Category.HasFlagUnsafe(ItemCategories.Limited);
			if (!isNormalCategory)
				return false;

			return true;
		}
	}

	public bool IsValidItemForCopy
	{
		get
		{
			if (this.Item == null)
				return false;

			if (this.Item.ModelBase == 0)
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

	private void OnOpenInConsoleGamesWikiClicked(object sender, RoutedEventArgs e)
	{
		this.OpenItemInFanSiteUrl("https://ffxiv.consolegameswiki.com/wiki/" + this.Item?.Name.Replace(" ", "_"));
	}

	private void OnOpenInGamerEscapeClicked(object sender, RoutedEventArgs e)
	{
		this.OpenItemInFanSiteUrl("https://ffxiv.gamerescape.com/wiki/" + this.Item?.Name.Replace(" ", "_"));
	}

	private void OnOpenInGarlandToolsClicked(object sender, RoutedEventArgs e)
	{
		this.OpenItemInFanSiteUrl("https://www.garlandtools.org/db/#item/" + this.Item?.RowId);
	}

	private void OpenItemInFanSiteUrl(string url)
	{
		if (this.Item == null)
			return;

		if (this.Item.ModelBase == 0)
			return;

		UrlUtility.Open(url);
	}

	private async void OnCopyItemNameClicked(object sender, RoutedEventArgs e)
	{
		if (this.Item == null)
			return;

		if (this.Item.ModelBase == 0)
			return;

		try
		{
			await ClipboardUtility.CopyToClipboardAsync(this.Item.Name);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Error copying item name to clipboard after 3 attempts.");
		}
	}

	private void OnResetSlotClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (this.Actor.Pinned == null)
			return;

		this.Actor.Pinned.RestoreCharacterBackup(PinnedActor.BackupModes.Original, this.Slot);
	}

	private void OnClearSlotClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor?.CanRefresh != true)
			return;

		this.ItemModel?.Clear(this.Actor.IsHuman);
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
		if (this.Actor?.CanRefresh != true)
			return;

		EquipmentSelector selector = new EquipmentSelector(this.Slot, this.Actor);
		SelectorDrawer.Show(selector, this.Item, (i) => this.SetItem(i, selector.AutoOffhand, selector.ForceMainModel, selector.ForceOffModel));
	}

	private void OnSlotMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (this.Actor?.CanRefresh != true)
			return;

		if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
		{
			this.ItemModel?.Clear(this.Actor.IsHuman);
		}
	}

	private void OnDyeMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (this.Actor?.CanRefresh != true || this.ItemModel == null)
			return;

		if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
		{
			this.ItemModel.Dye = 0;
		}
	}

	private void OnDye2MouseUp(object sender, MouseButtonEventArgs e)
	{
		if (this.Actor?.CanRefresh != true || this.ItemModel == null)
			return;

		if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
		{
			this.ItemModel.Dye2 = 0;
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
				&& ivm.EquipSlotCategory.Value.OffHand == -1)
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
				this.Dye2 = ItemUtility.NoneDye;
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

	private void OnDyeClick2(object sender, RoutedEventArgs e)
	{
		if (!this.CanDye)
			return;

		SelectorDrawer.Show<DyeSelector, IDye>(this.Dye2, (v) =>
		{
			if (v == null)
				return;

			if (this.ItemModel is ItemMemory item)
			{
				item.Dye2 = v.Id;
			}
			else if (this.ItemModel is WeaponMemory weapon)
			{
				weapon.Dye2 = v.Id;
			}
		});
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs? e)
	{
		if (this.lockViewModel)
			return;

		// Ignore changes from the Transform property
		// This is because weapon model transforms are constantly updating, which causes constant the item view refreshes
		if (e?.PropertyName == nameof(TransformMemory.Position) || e?.PropertyName == nameof(TransformMemory.Rotation) || e?.PropertyName == nameof(TransformMemory.Scale) || e?.PropertyName == nameof(WeaponModelMemory.Transform))
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

				if (this.Actor == null)
					throw new Exception("No Actor in item view");

				if (valueVm is ItemMemory itemVm)
				{
					IItem? item = ItemUtility.GetItem(slots, 0, itemVm.Base, itemVm.Variant, this.Actor.IsChocobo);
					IDye? dye;
					IDye? dye2;

					try
					{
						dye = GameDataService.Dyes.GetRow(itemVm.Dye);
					}
					catch (ArgumentOutOfRangeException)
					{
						dye = DyeUtility.NoneDye;
					}

					try
					{
						dye2 = GameDataService.Dyes.GetRow(itemVm.Dye2);
					}
					catch (ArgumentOutOfRangeException)
					{
						dye2 = DyeUtility.NoneDye;
					}

					await Dispatch.MainThread();

					this.Item = item;
					this.Dye = dye;
					this.Dye2 = dye2;
				}
				else if (valueVm is WeaponMemory weaponVm)
				{
					IItem? item = ItemUtility.GetItem(slots, weaponVm.Set, weaponVm.Base, weaponVm.Variant, this.Actor.IsChocobo);

					if (weaponVm.Set == 0)
						weaponVm.Dye = 0;

					IDye? dye;
					IDye? dye2;

					try
					{
						dye = GameDataService.Dyes.GetRow(weaponVm.Dye);
					}
					catch (ArgumentOutOfRangeException)
					{
						dye = DyeUtility.NoneDye;
					}

					try
					{
						dye2 = GameDataService.Dyes.GetRow(weaponVm.Dye2);
					}
					catch (ArgumentOutOfRangeException)
					{
						dye2 = DyeUtility.NoneDye;
					}

					await Dispatch.MainThread();

					this.Item = item;
					this.Dye = dye;
					this.Dye2 = dye2;
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
