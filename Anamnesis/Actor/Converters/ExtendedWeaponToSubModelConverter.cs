// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Converters;

using Anamnesis.Memory;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

[ValueConversion(typeof(ExtendedWeaponMemory), typeof(WeaponSubModelMemory))]
public class ExtendedWeaponToSubModelConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is ExtendedWeaponMemory extendedWeaponMemory)
		{
			return extendedWeaponMemory.SubModel;
		}

		return DependencyProperty.UnsetValue;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
