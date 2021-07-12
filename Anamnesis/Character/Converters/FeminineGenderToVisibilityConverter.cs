// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Converters
{
	using System;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Data;
	using Anamnesis.Memory;

	[ValueConversion(typeof(Customize.Genders), typeof(Visibility))]
	public class FeminineGenderToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Customize.Genders gender = (Customize.Genders)value;
			return gender == Customize.Genders.Feminine ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
