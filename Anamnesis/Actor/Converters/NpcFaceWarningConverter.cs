// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Converters;

using Anamnesis.Memory;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

public class NpcFaceWarningConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length != 4)
			throw new ArgumentException("The values array must have exactly four elements.", nameof(values));

		if (values[0] is ActorTypes type && values[1] is byte head && values[2] is ActorCustomizeMemory.Races race && values[3] is ActorCustomizeMemory.Genders gender)
		{
			bool isHroth = race == ActorCustomizeMemory.Races.Hrothgar;

			if (type == ActorTypes.BattleNpc || type == ActorTypes.EventNpc)
				return Visibility.Collapsed;

			// For all except Hrothgar, >4 is an NPC face.
			if (head > 4 && !isHroth)
				return Visibility.Visible;

			// For Hrothgar only, between 5 and 8 are player faces.
			// For Male Hrothgar specifically, faces 1-4 additionally are customizable.
			if (isHroth && (head > 8 || (gender == ActorCustomizeMemory.Genders.Feminine && head < 5)))
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
