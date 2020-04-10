// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule
{
	using System;
	using System.Windows.Media.Imaging;
	using ConceptMatrix.Services;

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
			BitmapImage logo = new BitmapImage();
			logo.BeginInit();
			logo.UriSource = new Uri("pack://application:,,,/AppearanceModule;component/Assets/Slots/" + self.ToString() + ".png");
			logo.EndInit();

			return logo;
		}
	}
}
