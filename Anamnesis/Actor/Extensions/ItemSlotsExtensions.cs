// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using Anamnesis.GameData;
using System;
using System.Windows.Media.Imaging;

public static class ItemSlotsExtensions
{
	public static string ToDisplayName(this ItemSlots self)
	{
		return self switch
		{
			ItemSlots.MainHand => "Main Hand",
			ItemSlots.OffHand => "Off Hand",
			ItemSlots.RightRing => "Right Ring",
			ItemSlots.LeftRing => "Left Ring",
			_ => self.ToString(),
		};
	}

	public static BitmapImage GetIcon(this ItemSlots self)
	{
		try
		{
			BitmapImage logo = new BitmapImage();
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
