// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters;

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

[ValueConversion(typeof(object[]), typeof(bool))]
public class HistoryIndexToOpacityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length == 3 && values[0] is int currentIndex && values[1] is ItemsControl itemsControl && values[2] != null)
		{
			var item = values[2];
			int entryIndex = itemsControl.Items.IndexOf(item);
			return entryIndex > currentIndex;
		}

		return false;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
