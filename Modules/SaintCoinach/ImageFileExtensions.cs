// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using ConceptMatrix.Services;
	using SaintCoinach.Imaging;

	public static class ImageFileExtensions
	{
		public static unsafe IImage ToIImage(this ImageFile file)
		{
			if (file == null)
				return null;

			byte[] src = file.GetData();
			SaintCoinach.Imaging.ImageFormat format = file.Format;
			int width = file.Width;
			int height = file.Height;
			byte[] argb = ImageConverter.GetA8R8G8B8(src, format, width, height);

			ImageWrapper wrapper;

			fixed (byte* p = argb)
			{
				IntPtr ptr = (IntPtr)p;
				Bitmap tempImage = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, ptr);
				wrapper = new ImageWrapper(tempImage);
			}

			return wrapper;
		}

		internal class ImageWrapper : IImage, IDisposable
		{
			private Bitmap bitmap;
			private IntPtr ptr;

			public ImageWrapper(Bitmap bmp)
			{
				this.bitmap = bmp;
				this.ptr = this.bitmap.GetHbitmap();
			}

			public IntPtr HBitmap
			{
				get
				{
					if (this.ptr == null)
						throw new Exception("Cannot access a disposed object");

					return this.ptr;
				}
			}

			public int Width
			{
				get
				{
					return this.bitmap.Width;
				}
			}

			public int Height
			{
				get
				{
					return this.bitmap.Height;
				}
			}

			public void Dispose()
			{
				this.bitmap = null;
			}
		}
	}
}
