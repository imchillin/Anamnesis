// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.WpfStyles
{
	using System;
	using System.Drawing;

	public class BitmapIImageSource : IImageSource
	{
		private Bitmap bmp;

		internal BitmapIImageSource(Bitmap img)
		{
			this.bmp = img;
		}

		public IImage GetImage()
		{
			return new BitmapImageWrapper(this.bmp);
		}

		private class BitmapImageWrapper : IImage
		{
			private Bitmap bmp;

			internal BitmapImageWrapper(Bitmap bmp)
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
				// Dont dispose the bitmap here
			}
		}
	}
}
