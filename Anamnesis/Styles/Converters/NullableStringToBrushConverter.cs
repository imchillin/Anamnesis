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
			return new SolidColorBrush(Colors.Transparent);
		}

		if (value is string colorString)
		{
			try
			{
				return (SolidColorBrush)(new BrushConverter().ConvertFromString(colorString) ?? new SolidColorBrush(Colors.Transparent));
			}
			catch (FormatException)
			{
				return new SolidColorBrush(Colors.Transparent);
			}
		}

		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
