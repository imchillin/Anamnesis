// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using Anamnesis.Memory;

[ValueConversion(typeof(Vector2D), typeof(Vector2D))]
public class Vector2DRadiansToDegreesConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		float x = 0;
		float y = 0;

		if (value is Vector2D vec)
		{
			x = vec.X * (180 / MathF.PI);
			y = vec.Y * (180 / MathF.PI);
		}

		return new Vector2D(x, y);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		float x = 0;
		float y = 0;

		if (value is Vector2D vec)
		{
			x = vec.X * (MathF.PI / 180);
			y = vec.Y * (MathF.PI / 180);
		}

		return new Vector2D(x, y);
	}
}
