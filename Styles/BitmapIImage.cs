// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles
{
	using System;
	using System.Drawing;

	public class BitmapIImage : IImage
	{
		private Bitmap bmp;

		internal BitmapIImage(Bitmap img)
		{
			this.bmp = img;
			this.Width = img.Width;
			this.Height = img.Height;
			this.HBitmap = img.GetHbitmap();
		}

		public IntPtr HBitmap
		{
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}
	}
}
