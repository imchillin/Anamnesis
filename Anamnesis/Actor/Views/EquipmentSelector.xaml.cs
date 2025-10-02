// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Utilities;
using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Keyboard;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;

public abstract class EquipmentSelectorDrawer : SelectorDrawer<IItem>
{
}

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
public partial class EquipmentSelector : EquipmentSelectorDrawer
{
	private static Classes s_classFilter = Classes.All;
	private static ItemCategories s_categoryFilter = ItemCategories.All;
	private static bool s_showLocked = true;
	private static bool s_autoOffhand = true;
	private static bool s_showFilters = false;
	private static bool s_forceMainModel = false;
	private static bool s_forceOffModel = false;
	private static SortModes s_sortMode = SortModes.Row;

	private readonly Memory.ActorMemory? actor;

	public EquipmentSelector(ItemSlots slot, Memory.ActorMemory? actor)
	{
		this.Slot = slot;
		this.actor = actor;

		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.JobFilterText.Text = s_classFilter.Describe();

		HotkeyService.RegisterHotkeyHandler("AppearancePage.ClearEquipment", this.ClearSlot);
	}

	public enum SortModes
	{
		Name,
		Row,
		Level,
	}

	// Suppress CA1822: The properties are used in WPF bindings. Do not set them to static.
#pragma warning disable CA1822
	public bool ShowFilters
	{
		get => s_showFilters;
		set => s_showFilters = value;
	}

	public bool AutoOffhand
	{
		get => s_autoOffhand;
		set => s_autoOffhand = value;
	}
#pragma warning restore CA1822

	public ItemSlots Slot { get; set; }
	public bool IsMainHandSlot => this.Slot == ItemSlots.MainHand;
	public bool IsOffHandSlot => this.Slot == ItemSlots.OffHand;
	public bool IsWeaponSlot => (this.Slot & ItemSlots.Weapons) != 0;
	public bool IsSmallclothesSlot => (this.Slot & (~ItemSlots.Head) & ItemSlots.Armor) != 0;

	public SortModes SortMode
	{
		get => s_sortMode;
		set
		{
			s_sortMode = value;
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
		get => s_classFilter;
		set
		{
			s_classFilter = value;
			this.JobFilterText.Text = value.Describe();
			this.FilterItems();
		}
	}

	public ItemCategories CategoryFilter
	{
		get => s_categoryFilter;
		set
		{
			s_categoryFilter = value;
			this.FilterItems();
		}
	}

	public bool ShowLocked
	{
		get => s_showLocked;
		set
		{
			s_showLocked = value;
			this.FilterItems();
		}
	}

	public bool ForceMainModel
	{
		get => s_forceMainModel;
		set
		{
			s_forceMainModel = value;
			this.FilterItems();
		}
	}

	public bool ForceOffModel
	{
		get => s_forceOffModel;
		set
		{
			s_forceOffModel = value;
			this.FilterItems();
		}
	}

	public override void OnClosed()
	{
		base.OnClosed();

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
			this.AddItems(GameDataService.Items.ToEnumerable());
			this.AddItems(GameDataService.Perform.ToEnumerable());
		}

		return Task.CompletedTask;
	}

	protected override int Compare(IItem itemA, IItem itemB)
	{
		if (itemA.IsFavorite && !itemB.IsFavorite)
			return -1;

		if (!itemA.IsFavorite && itemB.IsFavorite)
			return 1;

		// Push the Emperor's New Fists to the top of the list for weapons.
		if (this.IsWeaponSlot)
		{
			if (itemA == ItemUtility.EmperorsNewFists && itemB != ItemUtility.EmperorsNewFists)
				return -1;

			if (itemA != ItemUtility.EmperorsNewFists && itemB == ItemUtility.EmperorsNewFists)
				return 1;
		}

		return this.SortMode switch
		{
			SortModes.Name => itemA.Name.CompareTo(itemB.Name),
			SortModes.Row => itemA.RowId.CompareTo(itemB.RowId),
			SortModes.Level => itemA.EquipLevel.CompareTo(itemB.EquipLevel),
			_ => throw new NotImplementedException($"Sort mode {this.SortMode} not implemented"),
		};
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

		if (!HasClass(this.ClassFilter, item.EquipableClasses))
			return false;

		if (!this.ValidCategory(item))
			return false;

		if (!this.ShowLocked && item is Item ivm && !this.CanEquip(ivm))
			return false;

		return MatchesSearch(item, search);
	}

	private static bool HasClass(Classes a, Classes b)
	{
		foreach (Classes? job in Enum.GetValues<Classes>().Select(v => (Classes?)v))
		{
			if (job == null || job == Classes.None)
				continue;

			if (a.HasFlagUnsafe(job.Value) && b.HasFlagUnsafe(job.Value))
			{
				return true;
			}
		}

		return false;
	}

	private static bool MatchesSearch(IItem item, string[]? search = null)
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

	private bool ValidCategory(IItem item)
	{
		ItemCategories itemCategory = item.Category;

		// Include none category
		bool categoryFiltered = this.CategoryFilter.HasFlagUnsafe(ItemCategories.Standard) && itemCategory == ItemCategories.None;

		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.Standard) && itemCategory.HasFlagUnsafe(ItemCategories.Standard);
		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.Premium) && itemCategory.HasFlagUnsafe(ItemCategories.Premium);
		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.Limited) && itemCategory.HasFlagUnsafe(ItemCategories.Limited);
		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.Deprecated) && itemCategory.HasFlagUnsafe(ItemCategories.Deprecated);
		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.CustomEquipment) && itemCategory.HasFlagUnsafe(ItemCategories.CustomEquipment);
		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.Performance) && itemCategory.HasFlagUnsafe(ItemCategories.Performance);
		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.Modded) && item.Mod != null;
		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.Favorites) && item.IsFavorite;
		categoryFiltered |= this.CategoryFilter.HasFlagUnsafe(ItemCategories.Owned) && item.IsOwned;
		return categoryFiltered;
	}

	private bool CanEquip(Item item)
	{
		if (!item.EquipRestriction.IsValid || this.actor == null || this.actor.Customize == null)
			return true;

		return item.EquipRestriction.Value.CanEquip(this.actor.Customize.Race, this.actor.Customize.Gender);
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
