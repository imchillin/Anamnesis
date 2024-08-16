// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters;

using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Data;

[ValueConversion(typeof(Vector2), typeof(Vector2))]
public class Vector2DRadiansToDegreesConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		float x = 0;
		float y = 0;

		if (value is Vector2 vec)
		{
			x = vec.X * (180 / MathF.PI);
			y = vec.Y * (180 / MathF.PI);
		}

		return new Vector2(x, y);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		float x = 0;
		float y = 0;

		if (value is Vector2 vec)
		{
			x = vec.X * (MathF.PI / 180);
			y = vec.Y * (MathF.PI / 180);
		}

		return new Vector2(x, y);
	}
}
