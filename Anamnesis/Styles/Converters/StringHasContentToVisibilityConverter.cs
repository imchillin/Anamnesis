// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.WpfStyles.Converters
{
	using System;
	using System.Windows;
	using System.Windows.Data;

	[ValueConversion(typeof(string), typeof(Visibility))]
	public class StringHasContentToVisibilityConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string? val = value as string;
			return string.IsNullOrEmpty(val) ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
