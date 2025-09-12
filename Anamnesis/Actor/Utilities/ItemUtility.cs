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

	private static readonly ConcurrentDictionary<string, IItem> s_itemLookup = new();
	private static readonly ConcurrentDictionary<string, IItem> s_chocoboItemLookup = new();

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
			? s_chocoboItemLookup.GetOrAdd(lookupKey, _ => ChocoboItemSearch(slot, modelSet, modelBase, modelVariant))
			: s_itemLookup.GetOrAdd(lookupKey, _ => ItemSearch(slot, modelSet, modelBase, modelVariant));
	}

	public static IItem GetDummyItem(ushort modelSet, ushort modelBase, ushort modelVariant)
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

		return new DummyItem(modelSet, modelBase, modelVariant);
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

		return new DummyItem(modelSet, modelBase, modelVariant);
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

		return new DummyItem(modelSet, modelBase, modelVariant);
	}
}
