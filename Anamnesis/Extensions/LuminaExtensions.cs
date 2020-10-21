// Concept Matrix 3.
// Licensed under the MIT license.

namespace Lumina
{
	using System;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;

	using global::Lumina.Data.Files;
	using global::Lumina.Extensions;
	using LuminaData = global::Lumina.Lumina;

	public static class LuminaExtensions
	{
		public static IItem GetItem(ItemSlots slot, ulong val)
		{
			// ulong = unsigned 64-bit int = 8 byte
			byte[] bytes = BitConverter.GetBytes(val);

			short modelSet;
			short modelBase;
			short modelVariant;

			if (slot == ItemSlots.MainHand || slot == ItemSlots.OffHand)
			{
				modelSet = (short)val;
				modelBase = (short)(val >> 16);
				modelVariant = (short)(val >> 32);
			}
			else
			{
				modelSet = 0;
				modelBase = (short)val;
				modelVariant = (short)(val >> 16);
			}

			return ItemUtility.GetItem(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant);
		}

		public static ImageSource? GetImage(this LuminaData self, uint imageId)
		{
			return self.GetImage((int)imageId);
		}

		public static ImageSource? GetImage(this LuminaData self, int imageId)
		{
			TexFile tex = self.GetIcon(imageId);
			return tex.GetImage();
		}

		public static ImageSource? GetImage(this TexFile self)
		{
			if (self == null)
				return null;

			BitmapSource bmp = BitmapSource.Create(self.Header.Width, self.Header.Height, 96, 96, PixelFormats.Bgra32, null, self.ImageData, self.Header.Width * 4);
			bmp.Freeze();

			return bmp;
		}
	}
}
