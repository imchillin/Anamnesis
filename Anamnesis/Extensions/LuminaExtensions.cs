// © Anamnesis.
// Licensed under the MIT license.

namespace Lumina;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Serilog;

public static class LuminaExtensions
{
	public static IItem GetWeaponItem(ItemSlots slot, ulong val)
	{
		if (val == 0)
			return ItemUtility.NoneItem;

		short modelSet = (short)val;
		short modelBase = (short)(val >> 16);
		short modelVariant = (short)(val >> 32);

		if (modelSet < 0 || modelBase < 0 || modelVariant < 0)
		{
			Log.Warning($"Invalid item value: {val}");

			modelSet = 0;
			modelBase = 0;
			modelVariant = 0;
		}

		return ItemUtility.GetItem(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant, false);
	}

	public static IItem GetGearItem(ItemSlots slot, uint val)
	{
		if (val == 0)
			return ItemUtility.NoneItem;

		short modelSet = 0;
		short modelBase = (short)val;
		short modelVariant = (short)(val >> 16);

		if (modelSet < 0 || modelBase < 0 || modelVariant < 0)
		{
			Log.Warning($"Invalid item value: {val}");

			modelSet = 0;
			modelBase = 0;
			modelVariant = 0;
		}

		return ItemUtility.GetItem(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant, false);
	}

	public static void GetModel(ulong val, bool isWeapon, out ushort modelSet, out ushort modelBase, out ushort modelVariant)
	{
		if (isWeapon)
		{
			modelSet = (ushort)val;
			modelBase = (ushort)(val >> 16);
			modelVariant = (ushort)(val >> 32);
		}
		else
		{
			modelSet = 0;
			modelBase = (ushort)val;
			modelVariant = (ushort)(val >> 16);
		}
	}

	public static bool Contains(this Excel.Sheets.EquipSlotCategory self, ItemSlots slot)
	{
		return slot switch
		{
			ItemSlots.MainHand => self.MainHand == 1,
			ItemSlots.Head => self.Head == 1,
			ItemSlots.Body => self.Body == 1,
			ItemSlots.Hands => self.Gloves == 1,
			ItemSlots.Waist => self.Waist == 1,
			ItemSlots.Legs => self.Legs == 1,
			ItemSlots.Feet => self.Feet == 1,
			ItemSlots.OffHand => self.OffHand == 1,
			ItemSlots.Ears => self.Ears == 1,
			ItemSlots.Neck => self.Neck == 1,
			ItemSlots.Wrists => self.Wrists == 1,
			ItemSlots.RightRing => self.FingerR == 1,
			ItemSlots.LeftRing => self.FingerL == 1,
			ItemSlots.SoulCrystal => self.SoulCrystal == 1,
			_ => false,
		};
	}
}
