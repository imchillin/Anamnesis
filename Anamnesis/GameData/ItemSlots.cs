// © Anamnesis.
// Licensed under the MIT license.

using System;
using System.Windows.Media.Imaging;

namespace Anamnesis.GameData;

/// <summary>
/// Represents the slots that an item can be equipped in.
/// </summary>
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

public static class ItemSlotsExtensions
{
	/// <summary>
	/// Converts the item slot to a display name.
	/// </summary>
	/// <param name="self">The item slot to convert.</param>
	/// <returns>The display name of the item slot.</returns>
	public static string ToDisplayName(this ItemSlots self)
	{
		return self switch
		{
			ItemSlots.MainHand => "Main Hand",
			ItemSlots.OffHand => "Off Hand",
			ItemSlots.RightRing => "Right Ring",
			ItemSlots.LeftRing => "Left Ring",
			ItemSlots.SoulCrystal => "Soul Crystal",
			_ => self.ToString(),
		};
	}

	/// <summary>
	/// Gets the icon associated with the given item slot.
	/// </summary>
	/// <param name="self">The item slot to get the icon for.</param>
	/// <returns>The bitmap icon associated with the item slot.</returns>
	/// <exception cref="Exception">Thrown if the icon could not be loaded.</exception>
	public static BitmapImage GetIcon(this ItemSlots self)
	{
		try
		{
			var logo = new BitmapImage();
			logo.BeginInit();
			logo.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
			logo.UriSource = new Uri("pack://application:,,,/Anamnesis;component/Assets/Slots/" + self.ToString() + ".png");
			logo.EndInit();
			return logo;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to get icon for slot {self}", ex);
		}
	}
}
