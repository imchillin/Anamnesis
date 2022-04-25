// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Anamnesis.Services;
using Lumina.Data.Files;
using Serilog;

public class ImageReference
{
	public readonly uint ImageId;

	private WeakReference<ImageSource>? cachedImage;

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

		if (this.cachedImage != null && this.cachedImage.TryGetTarget(out img))
		{
			return img;
		}

		try
		{
			Log.Verbose($"Loading image {this.ImageId}");

			////TexFile? tex = GameDataService.LuminaData.GetIcon(this.ImageId);
			////string path = $"ui/icon/{this.ImageId / 1000u * 1000:000000}/{this.ImageId:000000}.tex";
			string path = $"ui/icon/{this.ImageId / 1000u * 1000:000000}/{this.ImageId:000000}_hr1.tex";
			TexFile? tex = GameDataService.LuminaData.GetFile<TexFile>(path);

			if (tex == null)
				return null;

			BitmapSource bmp = BitmapSource.Create(tex.Header.Width, tex.Header.Height, 96, 96, PixelFormats.Bgra32, null, tex.ImageData, tex.Header.Width * 4);
			bmp.Freeze();
			img = bmp;

			if (this.cachedImage == null)
				this.cachedImage = new WeakReference<ImageSource>(img);

			this.cachedImage.SetTarget(img);
			return img;
		}
		catch (Exception ex)
		{
			Log.Warning(ex, $"Failed to load Image: {this.ImageId} form lumina");
		}

		return null;
	}
}
