// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using System;
using System.Windows.Media.Imaging;
using Anamnesis.GameData;

public static class ItemSlotsExtensions
{
	public static string ToDisplayName(this ItemSlots self)
	{
		switch (self)
		{
			case ItemSlots.MainHand: return "Main Hand";
			case ItemSlots.OffHand: return "Off Hand";
			case ItemSlots.RightRing: return "Right Ring";
			case ItemSlots.LeftRing: return "Left Ring";
		}

		return self.ToString();
	}

	public static BitmapImage GetIcon(this ItemSlots self)
	{
		return GetIconAssetFromUri(new Uri("pack://application:,,,/Anamnesis;component/Assets/Slots/" + self.ToString() + ".png"));
	}

	public static BitmapImage GetGlassesIcon()
	{
		return GetIconAssetFromUri(new Uri("pack://application:,,,/Anamnesis;component/Assets/Slots/Glasses.png"));
	}

	public static BitmapImage GetIconAssetFromUri(Uri uri)
	{
		try
		{
			BitmapImage logo = new BitmapImage();
			logo.BeginInit();
			logo.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
			logo.UriSource = uri;
			logo.EndInit();
			return logo;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to get icon for {uri.OriginalString}", ex);
		}
	}
}
