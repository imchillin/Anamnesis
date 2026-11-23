// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

[ValueConversion(typeof(object), typeof(Brush))]
public class NullableStringToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			// Default color
			return Brushes.Transparent;
		}

		if (value is string colorString)
		{
			if (string.IsNullOrWhiteSpace(colorString))
				return Brushes.Transparent;

			try
			{
				return (SolidColorBrush)(new BrushConverter().ConvertFromString(colorString) ?? Brushes.Transparent);
			}
			catch (FormatException)
			{
				return Brushes.Transparent;
			}
		}

		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
