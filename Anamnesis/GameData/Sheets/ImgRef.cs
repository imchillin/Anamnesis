// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using Anamnesis.Services;
using Lumina.Data.Files;
using Lumina.Extensions;
using Serilog;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/// <summary>
/// Represents an image reference in the game data, retrieved from Lumina.
/// </summary>
public class ImgRef
{
	/// <summary> The target DPI of the image.</summary>
	private const int IMG_DPI = 96;

	/// <summary>Image cache.</summary>
	private static readonly ConditionalWeakTable<ImgRef, ImageSource> imageCache = [];

	/// <summary>
	/// Gets the image ID.
	/// </summary>
	public readonly uint ImageId;

	/// <summary>
	/// Initializes a new instance of the <see cref="ImgRef"/> class.
	/// </summary>
	/// <param name="imageId">An image ID.</param>
	public ImgRef(uint imageId)
	{
		this.ImageId = imageId;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ImgRef"/> class.
	/// </summary>
	/// <param name="imageId">An image ID.</param>
	public ImgRef(ushort imageId)
	{
		this.ImageId = imageId;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ImgRef"/> class.
	/// </summary>
	/// <param name="imageId">An image ID.</param>
	public ImgRef(int imageId)
	{
		this.ImageId = (uint)imageId;
	}

	/// <summary>Gets the image source from the reference's image identifier.</summary>
	/// <returns>A bitmap source or null if the image is not found.</returns>
	public ImageSource? GetImage()
	{
		if (this.ImageId == 0 || GameDataService.LuminaData == null)
			return null;

		if (imageCache.TryGetValue(this, out ImageSource? img))
			return img;

		try
		{
			Log.Verbose($"Loading image {this.ImageId}");
			TexFile? tex = GameDataService.LuminaData.GetHqIcon(this.ImageId);
			if (tex == null)
				return null;

			BitmapSource bmp = BitmapSource.Create(tex.Header.Width, tex.Header.Height, IMG_DPI, IMG_DPI, PixelFormats.Bgra32, null, tex.ImageData, tex.Header.Width * 4);
			bmp.Freeze();
			img = bmp;

			imageCache.Add(this, img);
			return img;
		}
		catch (Exception ex)
		{
			Log.Warning(ex, $"Failed to load Image: {this.ImageId} form lumina");
		}

		return null;
	}

	/// <summary>
	/// Gets the image source from the reference's image identifier.
	/// </summary>
	/// <param name="imageId">The image identifier.</param>
	/// <returns>A bitmap source or null if the image is not found.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ImageSource? GetImage(uint imageId)
	{
		ImgRef imgRef = new(imageId);
		return imgRef.GetImage();
	}

	/// <summary>
	/// Gets the image source from the reference's image identifier.
	/// </summary>
	/// <param name="imageId">The image identifier.</param>
	/// <returns>A bitmap source or null if the image is not found.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ImageSource? GetImage(ushort imageId)
	{
		ImgRef imgRef = new(imageId);
		return imgRef.GetImage();
	}

	/// <summary>
	/// Gets the image source from the reference's image identifier.
	/// </summary>
	/// <param name="imageId">The image identifier.</param>
	/// <returns>A bitmap source or null if the image is not found.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ImageSource? GetImage(int imageId)
	{
		ImgRef imgRef = new(imageId);
		return imgRef.GetImage();
	}
}
