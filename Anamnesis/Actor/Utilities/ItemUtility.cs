// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Utilities;

using Anamnesis.Actor.Items;
using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using System;
using System.Linq;

public static class ItemUtility
{
	public const uint EMPERORS_NEW_FISTS_ROWID = 13775;

	public static readonly DummyNoneItem NoneItem = new();
	public static readonly DummyNoneDye NoneDye = new();
	public static readonly NpcBodyItem NpcBodyItem = new();
	public static readonly EmperorsEquipItem EmperorsBodyItem = new();
	public static readonly EmperorsAccessoryItem EmperorsAccessoryItem = new();
	public static readonly InvisibleBodyItem InvisibileBodyItem = new();
	public static readonly InvisibleHeadItem InvisibileHeadItem = new();

	private static IItem? s_emperorsNewFists;
	private static ChocoboSkinItem? s_yellowChocoboSkin;
	private static ChocoboSkinItem? s_blackChocoboSkin;

	public static IItem EmperorsNewFists => s_emperorsNewFists ??= GameDataService.Items.GetRow(EMPERORS_NEW_FISTS_ROWID);
	public static ChocoboSkinItem YellowChocoboSkin => s_yellowChocoboSkin ??= new(GameDataService.Mounts.GetRow(1), 1);
	public static ChocoboSkinItem BlackChocoboSkin => s_blackChocoboSkin ??= new(GameDataService.Mounts.GetRow(1), 2);

	/// <summary>
	/// Searches the gamedata service item list for an item with the given model attributes.
	/// </summary>
	public static IItem GetItem(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant, bool isChocobo = false)
	{
		bool isWeaponSlot = (slot & ItemSlots.Weapons) != 0;
		if ((isWeaponSlot && modelSet == 0) || modelBase == 0)
			return NoneItem;

		ulong model = ExcelPageExtensions.ConvertToModel(modelSet, modelBase, modelVariant);

		if (model == NpcBodyItem.Model)
			return NpcBodyItem;

		if (model == InvisibileBodyItem.Model)
			return InvisibileBodyItem;

		if (model == InvisibileHeadItem.Model)
			return InvisibileHeadItem;

		return isChocobo
			? ChocoboItemSearch(slot, model)
			: ItemSearch(slot, model, isWeaponSlot);
	}

	private static IItem ChocoboItemSearch(ItemSlots slot, ulong model)
	{
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
				BuddyEquip.BuddyItem? item = slot switch
				{
					ItemSlots.Head => equip.Head,
					ItemSlots.Body => equip.Body,
					ItemSlots.Legs => equip.Feet,
					_ => null,
				};

				if (item != null && item.Model == model)
					return item;
			}
		}

		return new DummyItem(model, isWeapon: false);
	}

	private static IItem ItemSearch(ItemSlots slot, ulong model, bool isWeaponSlot)
	{
		foreach (uint rowId in GameDataService.ItemsByModel[model])
		{
			var tItem = GameDataService.Items.GetRow(rowId);

			if (isWeaponSlot ? !tItem.IsWeapon : !tItem.FitsInSlot(slot))
				continue;

			// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
			if (slot == ItemSlots.Wrists && tItem.Name.StartsWith("Promise of", StringComparison.Ordinal))
				continue;

			return tItem;
		}

		if (isWeaponSlot)
		{
			foreach (uint rowId in GameDataService.ItemsBySubModel[model])
			{
				var tItem = GameDataService.Items.GetRow(rowId);
				if (tItem.IsWeapon)
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

		return new DummyItem(model, isWeapon: isWeaponSlot);
	}
}
