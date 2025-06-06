// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Utilities;

using Anamnesis.Actor.Items;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using System.Collections.Concurrent;

public static class ItemUtility
{
	public static readonly DummyNoneItem NoneItem = new();
	public static readonly DummyNoneDye NoneDye = new();
	public static readonly NpcBodyItem NpcBodyItem = new();
	public static readonly EmperorsEquipItem EmperorsBodyItem = new();
	public static readonly EmperorsAccessoryItem EmperorsAccessoryItem = new();
	public static readonly InvisibleBodyItem InvisibileBodyItem = new();
	public static readonly InvisibleHeadItem InvisibileHeadItem = new();

	private static readonly ConcurrentDictionary<string, IItem> ItemLookup = new();
	private static readonly ConcurrentDictionary<string, IItem> ChocoboItemLookup = new();

	public static IItem EmperorsNewFists => GameDataService.Items.GetRow(13775);

	public static ChocoboSkinItem YellowChocoboSkin => new(GameDataService.Mounts.GetRow(1), 1);
	public static ChocoboSkinItem BlackChocoboSkin => new(GameDataService.Mounts.GetRow(1), 2);

	/// <summary>
	/// Searches the gamedata service item list for an item with the given model attributes.
	/// </summary>
	public static IItem GetItem(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant, bool isChocobo)
	{
		if ((modelBase == 0 || modelBase == 1) && modelVariant == 0)
			return NoneItem;

		if (modelBase == NpcBodyItem.ModelBase)
			return NpcBodyItem;

		string lookupKey = $"{slot}_{modelSet}_{modelBase}_{modelVariant}";

		return isChocobo
			? ChocoboItemLookup.GetOrAdd(lookupKey, _ => ChocoboItemSearch(slot, modelSet, modelBase, modelVariant))
			: ItemLookup.GetOrAdd(lookupKey, _ => ItemSearch(slot, modelSet, modelBase, modelVariant));
	}

	public static IItem GetDummyItem(uint rowId, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		var model = ExcelPageExtensions.ConvertToModel(modelSet, modelBase, modelVariant);

		if (NoneItem.Model == model)
			return NoneItem;

		if (NpcBodyItem.Model == model)
			return NpcBodyItem;

		if (InvisibileBodyItem.Model == model)
			return InvisibileBodyItem;

		if (InvisibileHeadItem.Model == model)
			return InvisibileHeadItem;

		return new DummyItem(rowId, modelSet, modelBase, modelVariant);
	}

	/// <summary>
	/// Attempts to locate an item based on the different properties associated with favoriting items
	/// It will first use the prefix to decide how to locate the item.
	/// - For chocobo skins, simply return one or the other based on the rowId.
	/// - For buddy equipment, get the buddy equip and then use the buddyEquipSlot parameter to decide which slot to return.
	/// - For one-off items, simply return the right item based on the rowId.
	/// - For all else, return the item from its GameData list based on its rowId.
	/// </summary>
	public static IItem? GetItemByItemFavoriteProperties(uint favoritePrefix, uint rowId, uint buddyEquipSlot)
	{
		ItemFavoriteCategory favoriteItemCategory = (ItemFavoriteCategory)favoritePrefix;

		switch(favoriteItemCategory)
		{
			case ItemFavoriteCategory.ChocoboSkin:
				// Two chocobo skins, based on the variant.
				if (rowId == 1)
					return YellowChocoboSkin;
				return BlackChocoboSkin;
			case ItemFavoriteCategory.BuddyEquipment:
				// Buddy equipment (barding) also requires selecting a slot. 
				// Get the buddy equip based on row id, look up the slot based on our arbitrary value, and then return the relevant buddy item.
				BuddyEquip buddyEquip = GameDataService.BuddyEquips.GetRow(rowId);
				ItemSlots slot = BuddyEquip.GetBuddyEquipItemSlotByIntAlias(buddyEquipSlot);

				if (buddyEquip.Head != null && buddyEquip.Head.Slot == slot)
					return buddyEquip.Head;

				if (buddyEquip.Body != null && buddyEquip.Body.Slot == slot)
					return buddyEquip.Body;

				if (buddyEquip.Feet != null && buddyEquip.Feet.Slot == slot)
					return buddyEquip.Feet;

				return null;
			case ItemFavoriteCategory.OneOffItem:
				// These values are set in these items rowId field with values that closely represent what the items are.
				return rowId switch
				{
					0 => NoneItem,
					1 => InvisibileHeadItem,
					2 => InvisibileBodyItem,
					52 => EmperorsAccessoryItem,
					279 => EmperorsBodyItem,
					9903 => NpcBodyItem,
					_ => null
				};
			case ItemFavoriteCategory.CustomEquipment:
				// Items (props) from Equipment.json, which get rowIds based on index.
				return GameDataService.Equipment.GetRow(rowId);
			case ItemFavoriteCategory.Perform:
				// Performance equips get a rowId from the Lumina sheet.
				return GameDataService.Perform.GetRow(rowId);
			default:
				// All other items get their rowId from the Lumina sheet.
				return GameDataService.Items.GetRow(rowId);
		}
	}

	/// <summary>
	/// Returns a composite ID containing the prefix prepended to the item's rowId.
	/// This prefix is an empty string if the item's favorite category is none. (applies to MOST items)
	/// </summary>
	public static uint GetCompositeIdForItemFavorite(IItem item)
	{
		if(item.FavoriteItemCategory.Equals(ItemFavoriteCategory.None))
		{
			// Don't derive a prefix if we don't need to. (For normal items.)
			return item.RowId;
		}
		else if(item.FavoriteItemCategory.Equals(ItemFavoriteCategory.BuddyEquipment))
		{
			// Prepend a prefix to the rowId to create a unique ID for this item among ALL other items.
			// For buddy equipment, we need to unique on the slot as well.
			// Append an arbitrary value to represent the slot in a single digit.
			uint favoriteItemCategoryPrefix = (uint)item.FavoriteItemCategory;
			uint buddyEquipSlotNum = ((BuddyEquip.BuddyItem)item).GetBuddyEquipSlotIntAlias();
			return uint.Parse(favoriteItemCategoryPrefix.ToString() + item.RowId.ToString() + buddyEquipSlotNum.ToString());
		}
		else
		{
			// Prepend a prefix to the rowId to create a unique ID for this item among ALL other items.
			uint favoriteItemCategoryPrefix = (uint)item.FavoriteItemCategory;
			return uint.Parse(favoriteItemCategoryPrefix.ToString() + item.RowId.ToString());
		}
	}

	/// <summary>
	/// Used as a simple method to compare two IItem objects by the properties that make up the
	/// composite Id for favoriting. This is only used to remove items from the respective favorites array.
	/// </summary>
	public static bool CompareItemFavoritesByCompositeId<T>(T x, T y)
	{
		if (x == null || y == null)
			return false;

		if (x is IItem iItemX && y is IItem iItemY)
		{
			if (!iItemX.FavoriteItemCategory.Equals(iItemY.FavoriteItemCategory))
				return false;

			if (!iItemX.RowId.Equals(iItemY.RowId))
				return false;

			if (iItemX.FavoriteItemCategory.Equals(ItemFavoriteCategory.BuddyEquipment))
			{
				uint xBuddyEquipSlotNum = ((BuddyEquip.BuddyItem)iItemX).GetBuddyEquipSlotIntAlias();
				uint yBuddyEquipSlotNum = ((BuddyEquip.BuddyItem)iItemY).GetBuddyEquipSlotIntAlias();
				return xBuddyEquipSlotNum.Equals(yBuddyEquipSlotNum);
			}

			return true;
		}
		return false;
	}

	private static IItem ChocoboItemSearch(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		var model = ExcelPageExtensions.ConvertToModel(modelSet, modelBase, modelVariant);

		if (slot == ItemSlots.Legs)
		{
			if (YellowChocoboSkin.Model == model)
				return YellowChocoboSkin;

			if (BlackChocoboSkin.Model == model)
				return BlackChocoboSkin;
		}
		else
		{
			foreach (BuddyEquip equip in GameDataService.BuddyEquips)
			{
				if (equip.Head != null && equip.Head.Slot == slot && equip.Head.Model == model)
					return equip.Head;

				if (equip.Body != null && equip.Body.Slot == slot && equip.Body.Model == model)
					return equip.Body;

				if (equip.Feet != null && equip.Feet.Slot == slot && equip.Feet.Model == model)
					return equip.Feet;
			}
		}

		return new DummyItem(0, modelSet, modelBase, modelVariant);
	}

	private static IItem ItemSearch(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		var model = ExcelPageExtensions.ConvertToModel(modelSet, modelBase, modelVariant);

		foreach (IItem tItem in GameDataService.Items)
		{
			if (slot == ItemSlots.MainHand || slot == ItemSlots.OffHand)
			{
				if (!tItem.IsWeapon)
					continue;
			}
			else
			{
				if (!tItem.FitsInSlot(slot))
					continue;
			}

			// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
			if (slot == ItemSlots.Wrists && tItem.Name.StartsWith("Promise of"))
				continue;

			if (tItem.Model == model)
				return tItem;

			if (slot == ItemSlots.MainHand || slot == ItemSlots.OffHand)
			{
				if (tItem.HasSubModel && tItem.SubModel == model)
					return tItem;
			}
		}

		foreach (IItem tItem in GameDataService.Equipment)
		{
			if (tItem.Model == model)
				return tItem;
		}

		foreach (IItem tItem in GameDataService.Perform)
		{
			if (tItem.Model == model)
				return tItem;
		}

		return new DummyItem(0, modelSet, modelBase, modelVariant);
	}
}
