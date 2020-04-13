// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Converters
{
	using System;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Interop;
	using System.Windows.Media.Imaging;

	[ValueConversion(typeof(IImage), typeof(BitmapSource))]
	public class IImageToImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			IImage img = value as IImage;

			if (img == null)
				return null;

			BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(img.HBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			source.Freeze();
			return source;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
