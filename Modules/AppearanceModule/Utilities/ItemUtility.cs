// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Anamnesis.AppearanceModule.Items;
	using Anamnesis.GameData;
	using Anamnesis.Services;

	public static class ItemUtility
	{
		public static readonly DummyNoneItem NoneItem = new DummyNoneItem();
		public static readonly DummyNoneDye NoneDye = new DummyNoneDye();
		public static readonly NpcBodyItem NpcbodyItem = new NpcBodyItem();

		/// <summary>
		/// Searches the gamedata service item list for an item with the given model attributes.
		/// </summary>
		public static IItem GetItem(ItemSlots slot, ushort modelSet, ushort modelBase, byte modelVariant)
		{
			if (modelBase == 0 && modelVariant == 0)
				return NoneItem;

			if (modelBase == NpcbodyItem.ModelBase)
				return NpcbodyItem;

			foreach (IItem tItem in GameDataService.Items.All)
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

			foreach (IItem tItem in Module.Props)
			{
				if (tItem.ModelSet == modelSet && tItem.ModelBase == modelBase && tItem.ModelVariant == modelVariant)
				{
					return tItem;
				}
			}

			return new DummyItem(modelSet, modelBase, modelVariant);
		}
	}
}
