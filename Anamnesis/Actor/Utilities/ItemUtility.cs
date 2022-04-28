// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Utilities;

using System.Collections.Concurrent;
using Anamnesis.Actor.Items;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Services;

public static class ItemUtility
{
	public static readonly DummyNoneItem NoneItem = new DummyNoneItem();
	public static readonly DummyNoneDye NoneDye = new DummyNoneDye();
	public static readonly NpcBodyItem NpcBodyItem = new NpcBodyItem();
	public static readonly InvisibleBodyItem InvisibileBodyItem = new InvisibleBodyItem();
	public static readonly InvisibleHeadItem InvisibileHeadItem = new InvisibleHeadItem();

	private static readonly ConcurrentDictionary<string, IItem> ItemLookup = new ConcurrentDictionary<string, IItem>();
	private static readonly ConcurrentDictionary<string, IItem> ChocoboItemLookup = new ConcurrentDictionary<string, IItem>();

	public static IItem EmperorsNewFists => GameDataService.Items.Get(13775);

	public static ChocoboSkinItem YellowChocoboSkin => new(GameDataService.Mounts.Get(1), 1);
	public static ChocoboSkinItem BlackChocoboSkin => new(GameDataService.Mounts.Get(1), 2);

	/// <summary>
	/// Searches the gamedata service item list for an item with the given model attributes.
	/// </summary>
	public static IItem GetItem(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant, bool isChocobo)
	{
		if ((modelBase == 0 || modelBase == 1) && modelVariant == 0)
			return NoneItem;

		if (modelBase == NpcBodyItem.ModelBase)
			return NpcBodyItem;

		string lookupKey = slot + "_" + modelSet + "_" + modelBase + "_" + modelVariant;

		if (isChocobo)
		{
			if (!ChocoboItemLookup.ContainsKey(lookupKey))
			{
				IItem item = ChocoboItemSearch(slot, modelSet, modelBase, modelVariant);
				ChocoboItemLookup.TryAdd(lookupKey, item);
			}

			return ChocoboItemLookup[lookupKey];
		}
		else
		{
			if (!ItemLookup.ContainsKey(lookupKey))
			{
				IItem item = ItemSearch(slot, modelSet, modelBase, modelVariant);
				ItemLookup.TryAdd(lookupKey, item);
			}

			return ItemLookup[lookupKey];
		}
	}

	public static IItem GetDummyItem(ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (NoneItem.IsModel(modelSet, modelBase, modelVariant))
			return NoneItem;

		if (NpcBodyItem.IsModel(modelSet, modelBase, modelVariant))
			return NpcBodyItem;

		if (InvisibileBodyItem.IsModel(modelSet, modelBase, modelVariant))
			return InvisibileBodyItem;

		if (InvisibileHeadItem.IsModel(modelSet, modelBase, modelVariant))
			return InvisibileHeadItem;

		return new DummyItem(modelSet, modelBase, modelVariant);
	}

	public static bool IsModel(this IItem item, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		return item.ModelSet == modelSet && item.ModelBase == modelBase && item.ModelVariant == modelVariant;
	}

	private static IItem ChocoboItemSearch(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		if (slot == ItemSlots.Legs)
		{
			if (YellowChocoboSkin.IsModel(modelSet, modelBase, modelVariant))
				return YellowChocoboSkin;

			if (BlackChocoboSkin.IsModel(modelSet, modelBase, modelVariant))
				return BlackChocoboSkin;
		}
		else
		{
			foreach (BuddyEquip equip in GameDataService.BuddyEquips)
			{
				if (equip.Head != null && equip.Head.Slot == slot && equip.Head.ModelSet == modelSet && equip.Head.ModelBase == modelBase && equip.Head.ModelVariant == modelVariant)
				{
					return equip.Head;
				}

				if (equip.Body != null && equip.Body.Slot == slot && equip.Body.ModelSet == modelSet && equip.Body.ModelBase == modelBase && equip.Body.ModelVariant == modelVariant)
				{
					return equip.Body;
				}

				if (equip.Feet != null && equip.Feet.Slot == slot && equip.Feet.ModelSet == modelSet && equip.Feet.ModelBase == modelBase && equip.Feet.ModelVariant == modelVariant)
				{
					return equip.Feet;
				}
			}
		}

		return new DummyItem(modelSet, modelBase, modelVariant);
	}

	private static IItem ItemSearch(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant)
	{
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
				{
					continue;
				}
			}

			// Big old hack, but we prefer the emperors bracelets to the promise bracelets (even though they are the same model)
			if (slot == ItemSlots.Wrists && tItem.Name.StartsWith("Promise of"))
				continue;

			if (slot == ItemSlots.MainHand || slot == ItemSlots.OffHand)
			{
				if (tItem.ModelSet == modelSet && tItem.ModelBase == modelBase && tItem.ModelVariant == modelVariant)
				{
					return tItem;
				}

				if (tItem.HasSubModel && tItem.SubModelSet == modelSet && tItem.SubModelBase == modelBase && tItem.SubModelVariant == modelVariant)
				{
					return tItem;
				}
			}
			else
			{
				if (tItem.ModelBase == modelBase && tItem.ModelVariant == modelVariant)
				{
					return tItem;
				}
			}
		}

		foreach (IItem tItem in GameDataService.Equipment)
		{
			if (tItem.ModelSet == modelSet && tItem.ModelBase == modelBase && tItem.ModelVariant == modelVariant)
			{
				return tItem;
			}
		}

		foreach (IItem tItem in GameDataService.Perform)
		{
			if (tItem.ModelSet == modelSet && tItem.ModelBase == modelBase && tItem.ModelVariant == modelVariant)
			{
				return tItem;
			}
		}

		return new DummyItem(modelSet, modelBase, modelVariant);
	}
}
