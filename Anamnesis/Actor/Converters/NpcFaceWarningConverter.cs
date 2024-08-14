// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Converters;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Anamnesis.Memory;

public class NpcFaceWarningConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length != 4)
			throw new ArgumentException();

		if (values[0] is ActorTypes type && values[1] is byte head && values[2] is ActorCustomizeMemory.Races race && values[3] is ActorCustomizeMemory.Genders gender)
		{
			bool isHrothF = race == ActorCustomizeMemory.Races.Hrothgar && gender == ActorCustomizeMemory.Genders.Feminine;

			if (type == ActorTypes.BattleNpc || type == ActorTypes.EventNpc)
				return Visibility.Collapsed;

			// For all except Fem Hrothgar, >4 is an NPC face.
			if (head > 4 && !isHrothF)
				return Visibility.Visible;

			// For Fem Hrothgar only, between 5 and 8 are player faces.
			if ((head < 5 || head > 8) && isHrothF)
				return Visibility.Visible;

			return Visibility.Collapsed;
		}
		else
		{
			return Visibility.Collapsed;
		}
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException("NpcFaceWarningConverter is a OneWay converter.");
	}
}
