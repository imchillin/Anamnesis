// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters
{
	using System;
	using System.Globalization;
	using System.Windows.Data;

	public class MultiBoolAndConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			foreach (object value in values)
			{
				if (value is bool boolValue)
				{
					if (!boolValue)
						return false;
				}
			}

			return true;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
		}
	}
}
