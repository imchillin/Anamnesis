// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Converters;

using Anamnesis.GameData.Sheets;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

[ValueConversion(typeof(ImgRef), typeof(ImageSource))]
public class ImageReferenceConverter : IValueConverter
{
	public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is ImgRef reference)
		{
			return reference.GetImage();
		}

		return null;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
