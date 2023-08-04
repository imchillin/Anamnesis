// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using Anamnesis.Memory;

[ValueConversion(typeof(Color), typeof(Color))]
[ValueConversion(typeof(Color4), typeof(Color4))]
public class SquaredRgbConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is Color rgb)
		{
			return new Color(MathF.Sqrt(rgb.R), MathF.Sqrt(rgb.G), MathF.Sqrt(rgb.B));
		}
		else if (value is Color4 rgba)
		{
			return new Color4(MathF.Sqrt(rgba.R), MathF.Sqrt(rgba.G), MathF.Sqrt(rgba.B), rgba.A);
		}
		else
		{
			return value;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is Color rgb)
		{
			return new Color(rgb.R * rgb.R, rgb.G * rgb.G, rgb.B * rgb.B);
		}
		else if (value is Color4 rgba)
		{
			return new Color4(rgba.R * rgba.R, rgba.G * rgba.G, rgba.B * rgba.B, rgba.A);
		}
		else
		{
			return value;
		}
	}
}
