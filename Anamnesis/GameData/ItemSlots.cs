// © Anamnesis.
// Licensed under the MIT license.

using System;

namespace Anamnesis.GameData;

[Flags]
public enum ItemSlots
{
	None = 0,
	MainHand = 1 << 0,
	OffHand = 1 << 1,
	Head = 1 << 2,
	Body = 1 << 3,
	Hands = 1 << 4,
	Waist = 1 << 5,
	Legs = 1 << 6,
	Feet = 1 << 7,
	Ears = 1 << 8,
	Neck = 1 << 9,
	Wrists = 1 << 10,
	RightRing = 1 << 11,
	LeftRing = 1 << 12,
	Glasses = 1 << 13,
	SoulCrystal = 1 << 14,

	Weapons = MainHand | OffHand,
	Armor = Head | Body | Hands | Waist | Legs | Feet,
	Accessories = Ears | Neck | Wrists | RightRing | LeftRing,
	All = MainHand | Head | Body | Hands | Waist | Legs | Feet | OffHand | Ears | Neck | Wrists | RightRing | LeftRing | Glasses | SoulCrystal,
}
