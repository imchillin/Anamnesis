// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

public class PointConverter : JsonConverter<Point>
{
	public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Vector");

		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 2)
			throw new FormatException();

		Point v = default;
		v.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);

		return v;
	}

	public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
	{
		var formatString = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", value.X, value.Y);
		writer.WriteStringValue(formatString);
	}
}
