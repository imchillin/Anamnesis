// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters;

using Anamnesis.Memory;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float SqrtOrNegative(float val) => (val > 0) ? MathF.Sqrt(val) : val;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float SqrOrNegative(float val) => (val > 0) ? val * val : val;
}
