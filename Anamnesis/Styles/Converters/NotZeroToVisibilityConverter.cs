// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters
{
	using System;
	using System.Windows;
	using System.Windows.Data;

	[ValueConversion(typeof(object), typeof(Visibility))]
	public class NotZeroToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isZero = true;

			if (value is int intV)
				isZero = intV == 0;

			if (value is float floatV)
				isZero = floatV == 0;

			if (value is double doubleV)
				isZero = doubleV == 0;

			return isZero ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
