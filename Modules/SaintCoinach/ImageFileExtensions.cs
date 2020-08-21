// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using SaintCoinach.Imaging;

	public static class ImageFileExtensions
	{
		public static IImageSource ToIImage(this ImageFile file)
		{
			if (file == null)
				return null;

			return new ImageSourceWrapper(file);
		}

		internal class ImageSourceWrapper : IImageSource
		{
			private ImageFile file;

			public ImageSourceWrapper(ImageFile file)
			{
				this.file = file;
			}

			public unsafe IImage GetImage()
			{
				byte[] src = this.file.GetData();
				SaintCoinach.Imaging.ImageFormat format = this.file.Format;
				int width = this.file.Width;
				int height = this.file.Height;
				byte[] argb = ImageConverter.GetA8R8G8B8(src, format, width, height);

				Bitmap bmp = null;

				try
				{
					fixed (byte* p = argb)
					{
						IntPtr ptr = (IntPtr)p;
						bmp = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, ptr);
					}
				}
				catch (Exception)
				{
				}

				if (bmp == null)
					return null;

				return new ImageWrapper(bmp);
			}
		}

		internal class ImageWrapper : IImage
		{
			private Bitmap bmp;

			internal ImageWrapper(Bitmap bmp)
			{
				this.bmp = bmp;
			}

			public IntPtr HBitmap
			{
				get
				{
					return this.bmp.GetHbitmap();
				}
			}

			public void Dispose()
			{
				this.bmp.Dispose();
			}
		}
	}
}
