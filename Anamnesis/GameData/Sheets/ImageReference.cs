// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Anamnesis.Services;
	using Lumina.Data.Files;
	using Lumina.Extensions;
	using Serilog;

	public class ImageReference
	{
		public readonly uint ImageId;

		private static readonly Dictionary<uint, WeakReference<ImageSource>> ImageCache = new Dictionary<uint, WeakReference<ImageSource>>();

		public ImageReference(uint imageId)
		{
			this.ImageId = imageId;
		}

		public ImageReference(ushort imageId)
		{
			this.ImageId = imageId;
		}

		public ImageReference(int imageId)
		{
			this.ImageId = (uint)imageId;
		}

		public ImageSource? GetImageSource()
		{
			if (this.ImageId == 0 || GameDataService.LuminaData == null)
				return null;

			ImageSource? img;
			WeakReference<ImageSource>? imgRef;
			if (ImageCache.TryGetValue(this.ImageId, out imgRef))
			{
				if (imgRef.TryGetTarget(out img) && img != null)
				{
					return img;
				}
			}

			Log.Verbose($"Image {this.ImageId} not in cache. loaing.");

			TexFile? tex = GameDataService.LuminaData.GetIcon(this.ImageId);

			if (tex == null)
				return null;

			BitmapSource bmp = BitmapSource.Create(tex.Header.Width, tex.Header.Height, 96, 96, PixelFormats.Bgra32, null, tex.ImageData, tex.Header.Width * 4);
			bmp.Freeze();
			img = bmp;

			imgRef = new WeakReference<ImageSource>(img);
			ImageCache.Add(this.ImageId, imgRef);
			return img;
		}
	}
}
