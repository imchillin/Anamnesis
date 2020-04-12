// Concept Matrix 3.
// Licensed under the MIT license.

namespace System.Drawing
{
	using ConceptMatrix;
	using ConceptMatrix.WpfStyles;

	public static class BitmapExtensions
	{
		public static IImage ToIImage(this Bitmap self)
		{
			return new BitmapIImage(self);
		}
	}
}
