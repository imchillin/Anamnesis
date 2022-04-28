// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Converters;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Anamnesis.Memory;

public class MinionGposeWarningConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length != 2)
			throw new ArgumentException();

		if (values[0] is ActorTypes type && values[1] is int model)
		{
			if (type != ActorTypes.Companion)
				return Visibility.Collapsed;

			if (model == 0)
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
		throw new NotSupportedException("MinionGposeWarningConverter is a OneWay converter.");
	}
}
