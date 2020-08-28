// Concept Matrix 3.
// Licensed under the MIT license.

namespace System.Drawing
{
	using Anamnesis;
	using Anamnesis.WpfStyles;

	public static class BitmapExtensions
	{
		public static IImageSource ToIImage(this Bitmap self)
		{
			return new BitmapIImageSource(self);
		}
	}
}
