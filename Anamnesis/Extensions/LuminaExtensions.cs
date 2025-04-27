// © Anamnesis.
// Licensed under the MIT license.

namespace Lumina;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using System.Runtime.CompilerServices;

public static class LuminaExtensions
{
	public static IItem GetWeaponItem(ItemSlots slot, ulong val)
	{
		if (val == 0)
			return ItemUtility.NoneItem;

		GetModel(val, true, out ushort modelSet, out ushort modelBase, out ushort modelVariant);
		return ItemUtility.GetItem(slot, modelSet, modelBase, modelVariant, false);
	}

	public static IItem GetGearItem(ItemSlots slot, uint val)
	{
		if (val == 0)
			return ItemUtility.NoneItem;

		GetModel(val, false, out ushort modelSet, out ushort modelBase, out ushort modelVariant);
		return ItemUtility.GetItem(slot, modelSet, modelBase, modelVariant, false);
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			ItemSlots.Weapons => self.MainHand == 1 || self.OffHand == 1,
			ItemSlots.Armor => self.Head == 1 || self.Body == 1 || self.Gloves == 1 || self.Waist == 1 || self.Legs == 1 || self.Feet == 1,
			ItemSlots.Accessories => self.Ears == 1 || self.Neck == 1 || self.Wrists == 1 || self.FingerR == 1 || self.FingerL == 1,
			ItemSlots.All => self.MainHand == 1 || self.Head == 1 || self.Body == 1 || self.Gloves == 1 || self.Waist == 1 || self.Legs == 1 || self.Feet == 1 || self.OffHand == 1 || self.Ears == 1 || self.Neck == 1 || self.Wrists == 1 || self.FingerR == 1 || self.FingerL == 1 || self.SoulCrystal == 1,
			_ => false,
		};
	}

	public static ItemSlots GetItemSlots(this Excel.Sheets.EquipSlotCategory self)
	{
		ItemSlots result = ItemSlots.None;

		if (self.MainHand == 1)
			result |= ItemSlots.MainHand;

		if (self.OffHand == 1)
			result |= ItemSlots.OffHand;

		if (self.Head == 1)
			result |= ItemSlots.Head;

		if (self.Body == 1)
			result |= ItemSlots.Body;

		if (self.Gloves == 1)
			result |= ItemSlots.Hands;

		if (self.Waist == 1)
			result |= ItemSlots.Waist;

		if (self.Legs == 1)
			result |= ItemSlots.Legs;

		if (self.Feet == 1)
			result |= ItemSlots.Feet;

		if (self.Ears == 1)
			result |= ItemSlots.Ears;

		if (self.Neck == 1)
			result |= ItemSlots.Neck;

		if (self.Wrists == 1)
			result |= ItemSlots.Wrists;

		if (self.FingerR == 1)
			result |= ItemSlots.RightRing;

		if (self.FingerL == 1)
			result |= ItemSlots.LeftRing;

		if (self.SoulCrystal == 1)
			result |= ItemSlots.SoulCrystal;

		return result;
	}
}
