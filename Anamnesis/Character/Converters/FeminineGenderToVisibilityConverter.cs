// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Converters
{
	using System;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Data;
	using Anamnesis.Memory;

	[ValueConversion(typeof(Appearance.Genders), typeof(Visibility))]
	public class FeminineGenderToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Appearance.Genders gender = (Appearance.Genders)value;
			return gender == Appearance.Genders.Feminine ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
