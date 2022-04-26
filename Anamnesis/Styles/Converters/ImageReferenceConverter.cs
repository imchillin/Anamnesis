// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Anamnesis.GameData.Sheets;

[ValueConversion(typeof(ImageReference), typeof(ImageSource))]
public class ImageReferenceConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is ImageReference reference)
		{
			return reference.GetImageSource();
		}

		return null;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
