// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters
{
	using System;
	using System.Windows;
	using System.Windows.Data;

	[ValueConversion(typeof(object), typeof(Visibility))]
	public class NullToVisibilityConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is string str)
			{
				if (string.IsNullOrEmpty(str))
				{
					value = null;
				}
			}

			return value == null ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
