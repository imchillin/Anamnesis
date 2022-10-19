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
		if (values.Length != 2)
			throw new ArgumentException();

		if (values[0] is ActorTypes type && values[1] is byte head)
		{
			if (type == ActorTypes.BattleNpc || type == ActorTypes.EventNpc)
				return Visibility.Collapsed;

			if (head > 5)
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
