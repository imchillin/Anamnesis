// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Converters;

using Anamnesis.Memory;
using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(object), typeof(ActorMemory))]
public class ActorMemoryCastConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value as ActorMemory;
	}

	public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
			return null;

		if (targetType.IsAssignableFrom(value.GetType()))
			return value;

		return null;
	}
}