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
			return new Color(SqrtOrNegative(rgb.R), SqrtOrNegative(rgb.G), SqrtOrNegative(rgb.B));
		}
		else if (value is Color4 rgba)
		{
			return new Color4(SqrtOrNegative(rgba.R), SqrtOrNegative(rgba.G), SqrtOrNegative(rgba.B), rgba.A);
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
			return new Color(SqrOrNegative(rgb.R), SqrOrNegative(rgb.G), SqrOrNegative(rgb.B));
		}
		else if (value is Color4 rgba)
		{
			return new Color4(SqrOrNegative(rgba.R), SqrOrNegative(rgba.G), SqrOrNegative(rgba.B), rgba.A);
		}
		else
		{
			return value;
		}
	}

	private static float SqrtOrNegative(float val)
	{
		if (val > 0)
		{
			return MathF.Sqrt(val);
		}

		return val;
	}

	private static float SqrOrNegative(float val)
	{
		if (val > 0)
		{
			return val * val;
		}

		return val;
	}
}
