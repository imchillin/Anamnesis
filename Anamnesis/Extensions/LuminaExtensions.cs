// © Anamnesis.
// Licensed under the MIT license.

namespace Lumina;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Lumina.Excel.GeneratedSheets;
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

	public static bool Contains(this EquipSlotCategory self, ItemSlots slot)
	{
		switch (slot)
		{
			case ItemSlots.MainHand: return self.MainHand == 1;
			case ItemSlots.Head: return self.Head == 1;
			case ItemSlots.Body: return self.Body == 1;
			case ItemSlots.Hands: return self.Gloves == 1;
			case ItemSlots.Waist: return self.Waist == 1;
			case ItemSlots.Legs: return self.Legs == 1;
			case ItemSlots.Feet: return self.Feet == 1;
			case ItemSlots.OffHand: return self.OffHand == 1;
			case ItemSlots.Ears: return self.Ears == 1;
			case ItemSlots.Neck: return self.Neck == 1;
			case ItemSlots.Wrists: return self.Wrists == 1;
			case ItemSlots.RightRing: return self.FingerR == 1;
			case ItemSlots.LeftRing: return self.FingerL == 1;
			case ItemSlots.SoulCrystal: return self.SoulCrystal == 1;
		}

		return false;
	}
}
