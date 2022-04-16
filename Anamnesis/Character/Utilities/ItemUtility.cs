// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Utilities
{
	using System.Collections.Concurrent;
	using Anamnesis.Character.Items;
	using Anamnesis.GameData;
	using Anamnesis.Services;

	public static class ItemUtility
	{
		public static readonly DummyNoneItem NoneItem = new DummyNoneItem();
		public static readonly DummyNoneDye NoneDye = new DummyNoneDye();
		public static readonly NpcBodyItem NpcBodyItem = new NpcBodyItem();
		public static readonly InvisibleBodyItem InvisibileBodyItem = new InvisibleBodyItem();
		public static readonly InvisibleHeadItem InvisibileHeadItem = new InvisibleHeadItem();

		private static readonly ConcurrentDictionary<string, IItem> ItemLookup = new ConcurrentDictionary<string, IItem>();

		public static IItem EmperorsNewFists => GameDataService.Items.Get(13775);

		/// <summary>
		/// Searches the gamedata service item list for an item with the given model attributes.
		/// </summary>
		public static IItem GetItem(ItemSlots slot, ushort modelSet, ushort modelBase, ushort modelVariant)
		{
			if (modelBase == 0 && modelVariant == 0)
				return NoneItem;

			if (modelBase == NpcBodyItem.ModelBase)
				return NpcBodyItem;

			string lookupKey = slot + "_" + modelSet + "_" + modelBase + "_" + modelVariant;
			if (!ItemLookup.ContainsKey(lookupKey))
			{
				IItem item = ItemSearch(slot, modelSet, modelBase, modelVariant);
				ItemLookup.TryAdd(lookupKey, item);
			}

			return ItemLookup[lookupKey];
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

			if (GameDataService.Props != null)
			{
				foreach (IItem tItem in GameDataService.Props)
				{
					if (tItem.ModelSet == modelSet && tItem.ModelBase == modelBase && tItem.ModelVariant == modelVariant)
					{
						return tItem;
					}
				}
			}

			if (GameDataService.Perform != null)
			{
				foreach (IItem tItem in GameDataService.Perform)
				{
					if (tItem.ModelSet == modelSet && tItem.ModelBase == modelBase && tItem.ModelVariant == modelVariant)
					{
						return tItem;
					}
				}
			}

			return new DummyItem(modelSet, modelBase, modelVariant);
		}
	}
}
