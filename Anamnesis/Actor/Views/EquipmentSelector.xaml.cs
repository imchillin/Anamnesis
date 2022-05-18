// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Keyboard;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using XivToolsWpf;

public abstract class EquipmentSelectorDrawer : SelectorDrawer<IItem>
{
}

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
public partial class EquipmentSelector : EquipmentSelectorDrawer
{
	private static Classes classFilter = Classes.All;
	private static ItemCategories categoryFilter = ItemCategories.All;
	private static bool showLocked = true;
	private static bool autoOffhand = true;
	private static bool showFilters = false;
	private static bool forceMainModel = false;
	private static bool forceOffModel = false;
	private static SortModes sortMode = SortModes.Row;

	private readonly Memory.ActorMemory? actor;

	public EquipmentSelector(ItemSlots slot, Memory.ActorMemory? actor)
	{
		this.Slot = slot;
		this.actor = actor;

		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.JobFilterText.Text = classFilter.Describe();

		HotkeyService.RegisterHotkeyHandler("AppearancePage.ClearEquipment", this.ClearSlot);
	}

	public enum SortModes
	{
		Name,
		Row,
		Level,
	}

	public bool ShowFilters
	{
		get => showFilters;
		set => showFilters = value;
	}

	public ItemSlots Slot { get; set; }
	public bool IsMainHandSlot => this.Slot == ItemSlots.MainHand;
	public bool IsOffHandSlot => this.Slot == ItemSlots.OffHand;
	public bool IsWeaponSlot => this.Slot == ItemSlots.MainHand || this.Slot == ItemSlots.OffHand;
	public bool IsSmallclothesSlot => this.Slot > ItemSlots.Head && this.Slot <= ItemSlots.OffHand;

	public SortModes SortMode
	{
		get => sortMode;
		set
		{
			sortMode = value;
			this.FilterItems();
		}
	}

	public int SortModeInt
	{
		get => (int)this.SortMode;
		set => this.SortMode = (SortModes)value;
	}

	public Classes ClassFilter
	{
		get => classFilter;
		set
		{
			classFilter = value;
			this.JobFilterText.Text = value.Describe();
			this.FilterItems();
		}
	}

	public ItemCategories CategoryFilter
	{
		get => categoryFilter;
		set
		{
			categoryFilter = value;
			this.FilterItems();
		}
	}

	public bool ShowLocked
	{
		get => showLocked;
		set
		{
			showLocked = value;
			this.FilterItems();
		}
	}

	public bool AutoOffhand
	{
		get => autoOffhand;
		set => autoOffhand = value;
	}

	public bool ForceMainModel
	{
		get => forceMainModel;
		set
		{
			forceMainModel = value;
			this.FilterItems();
		}
	}

	public bool ForceOffModel
	{
		get => forceOffModel;
		set
		{
			forceOffModel = value;
			this.FilterItems();
		}
	}

	public override void OnClosed()
	{
		HotkeyService.ClearHotkeyHandler("AppearancePage.ClearEquipment", this);
	}

	protected override Task LoadItems()
	{
		if (this.actor?.IsChocobo == true)
		{
			this.AddItem(ItemUtility.NoneItem);
			this.AddItem(ItemUtility.YellowChocoboSkin);
			this.AddItem(ItemUtility.BlackChocoboSkin);

			foreach (BuddyEquip buddyEquip in GameDataService.BuddyEquips)
			{
				if (buddyEquip.Head != null)
					this.AddItem(buddyEquip.Head);

				if (buddyEquip.Body != null)
					this.AddItem(buddyEquip.Body);

				if (buddyEquip.Feet != null)
					this.AddItem(buddyEquip.Feet);
			}
		}
		else
		{
			if (!this.IsMainHandSlot)
				this.AddItem(ItemUtility.NoneItem);

			this.AddItem(ItemUtility.NpcBodyItem);
			this.AddItem(ItemUtility.InvisibileBodyItem);
			this.AddItem(ItemUtility.InvisibileHeadItem);

			this.AddItems(GameDataService.Equipment);
			this.AddItems(GameDataService.Items);
			this.AddItems(GameDataService.Perform);
		}

		return Task.CompletedTask;
	}

	protected override int Compare(IItem itemA, IItem itemB)
	{
		if (itemA.IsFavorite && !itemB.IsFavorite)
		{
			return -1;
		}
		else if (!itemA.IsFavorite && itemB.IsFavorite)
		{
			return 1;
		}

		// Push the Emperor's New Fists to the top of the list for weapons.
		if (this.IsWeaponSlot)
		{
			if (itemA == ItemUtility.EmperorsNewFists && itemB != ItemUtility.EmperorsNewFists)
			{
				return -1;
			}
			else if (itemA != ItemUtility.EmperorsNewFists && itemB == ItemUtility.EmperorsNewFists)
			{
				return 1;
			}
		}

		switch (this.SortMode)
		{
			case SortModes.Name: return itemA.Name.CompareTo(itemB.Name);
			case SortModes.Row: return itemA.RowId.CompareTo(itemB.RowId);
			case SortModes.Level: return itemA.EquipLevel.CompareTo(itemB.EquipLevel);
		}

		throw new NotImplementedException($"Sort mode {this.SortMode} not implemented");
	}

	protected override bool Filter(IItem item, string[]? search)
	{
		// skip items without names
		if (string.IsNullOrEmpty(item.Name))
			return false;

		if (this.IsWeaponSlot)
		{
			////if (this.Slot == ItemSlots.OffHand && !forceMainModel && !item.HasSubModel)
			////	return false;

			if (!item.IsWeapon)
				return false;
		}
		else
		{
			if (!item.FitsInSlot(this.Slot))
				return false;
		}

		if (!this.HasClass(this.ClassFilter, item.EquipableClasses))
			return false;

		if (!this.ValidCategory(item))
			return false;

		if (!this.ShowLocked && item is Item ivm && !this.CanEquip(ivm))
			return false;

		return this.MatchesSearch(item, search);
	}

	private bool HasClass(Classes a, Classes b)
	{
		foreach (Classes? job in Enum.GetValues(typeof(Classes)))
		{
			if (job == null || job == Classes.None)
				continue;

			if (a.HasFlag(job) && b.HasFlag(job))
			{
				return true;
			}
		}

		return false;
	}

	private bool ValidCategory(IItem item)
	{
		ItemCategories itemCategory = item.Category;

		// Include none category
		bool categoryFiltered = this.CategoryFilter.HasFlag(ItemCategories.Standard) && itemCategory == ItemCategories.None;

		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Standard) && itemCategory.HasFlag(ItemCategories.Standard);
		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Premium) && itemCategory.HasFlag(ItemCategories.Premium);
		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Limited) && itemCategory.HasFlag(ItemCategories.Limited);
		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Deprecated) && itemCategory.HasFlag(ItemCategories.Deprecated);
		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.CustomEquipment) && itemCategory.HasFlag(ItemCategories.CustomEquipment);
		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Performance) && itemCategory.HasFlag(ItemCategories.Performance);
		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Modded) && item.Mod != null;
		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Favorites) && item.IsFavorite;
		categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Owned) && item.IsOwned;
		return categoryFiltered;
	}

	private bool CanEquip(Item item)
	{
		if (item.EquipRestriction == null || this.actor == null || this.actor.Customize == null)
			return true;

		return item.EquipRestriction.CanEquip(this.actor.Customize.Race, this.actor.Customize.Gender);
	}

	private bool MatchesSearch(IItem item, string[]? search = null)
	{
		bool matches = false;

		matches |= SearchUtility.Matches(item.Name, search);
		matches |= SearchUtility.Matches(item.Description, search);
		matches |= SearchUtility.Matches(item.ModelSet.ToString(), search);
		matches |= SearchUtility.Matches(item.ModelBase.ToString(), search);
		matches |= SearchUtility.Matches(item.ModelVariant.ToString(), search);

		if (item.HasSubModel)
		{
			matches |= SearchUtility.Matches(item.SubModelSet.ToString(), search);
			matches |= SearchUtility.Matches(item.SubModelBase.ToString(), search);
			matches |= SearchUtility.Matches(item.SubModelVariant.ToString(), search);
		}

		matches |= SearchUtility.Matches(item.RowId.ToString(), search);

		if (item.Mod != null && item.Mod.ModPack != null)
		{
			matches |= SearchUtility.Matches(item.Mod.ModPack.Name, search);
		}

		return matches;
	}

	private void ClearSlot()
	{
		this.OnClearClicked();
	}

	private void OnClearClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		if (this.IsMainHandSlot)
		{
			this.Value = ItemUtility.EmperorsNewFists;
		}
		else
		{
			this.Value = ItemUtility.NoneItem;
		}

		this.RaiseSelectionChanged();
	}

	private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
	{
		if (this.IsSmallclothesSlot)
		{
			this.Value = ItemUtility.NpcBodyItem;
		}
		else
		{
			this.Value = ItemUtility.NoneItem;
		}

		this.RaiseSelectionChanged();
	}
}
