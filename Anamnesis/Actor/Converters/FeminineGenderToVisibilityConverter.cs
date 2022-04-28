// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Converters;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Anamnesis.Memory;

[ValueConversion(typeof(ActorCustomizeMemory.Genders), typeof(Visibility))]
public class FeminineGenderToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		ActorCustomizeMemory.Genders gender = (ActorCustomizeMemory.Genders)value;
		return gender == ActorCustomizeMemory.Genders.Feminine ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
