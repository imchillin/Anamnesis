// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Serialization.Converters
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using Anamnesis.Memory.Offsets;

	#pragma warning disable SA1011

	public class OffsetConverter : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(Offset).IsAssignableFrom(typeToConvert);
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (typeof(FlagOffset).IsAssignableFrom(typeToConvert))
			{
				return new FlagOffsetConverter();
			}
			else
			{
				return new BasicOffsetConverter();
			}
		}

		public class FlagOffsetConverter : JsonConverter<FlagOffset>
		{
			public static byte[]? ToBytes(string? str)
			{
				if (string.IsNullOrEmpty(str))
					return null;

				string[] parts = str.Split(',');
				List<byte> values = new List<byte>();
				foreach (string part in parts)
				{
					string strngVal = part.Trim(' ');

					if (!strngVal.StartsWith("0x"))
						throw new FormatException("Flag byte values must start with '0x'");

					values.Add(byte.Parse(strngVal.Substring(2), NumberStyles.HexNumber));
				}

				return values.ToArray();
			}

			public static string ToString(byte[] data)
			{
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < data.Length; i++)
				{
					if (i > 0 && i < data.Length)
						builder.Append(", ");

					builder.Append("0x");
					builder.Append(data[i].ToString("X2"));
				}

				return builder.ToString();
			}

			public override FlagOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				ulong[]? offsets = null;
				byte[]? on = null;
				byte[]? off = null;

				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndObject)
					{
						if (offsets == null)
							throw new JsonException("Flag offsets must have 'offsets' value");

						if (on == null)
							throw new JsonException("Flag offsets must have 'on' value");

						if (off == null)
							throw new JsonException("Flag offsets must have 'off' value");

						return new FlagOffset(offsets, on, off);
					}

					if (reader.TokenType != JsonTokenType.PropertyName)
						throw new JsonException();

					string propertyName = reader.GetString();

					if (propertyName == nameof(FlagOffset.On))
					{
						reader.Read();
						on = ToBytes(reader.GetString());
					}
					else if (propertyName == nameof(FlagOffset.Off))
					{
						reader.Read();
						off = ToBytes(reader.GetString());
					}
					else if (propertyName == nameof(FlagOffset.Offsets))
					{
						reader.Read();
						offsets = BasicOffsetConverter.ToOffsets(reader.GetString());
					}
				}

				throw new JsonException();
			}

			public override void Write(Utf8JsonWriter writer, FlagOffset value, JsonSerializerOptions options)
			{
				writer.WriteStartObject();

				writer.WritePropertyName(nameof(FlagOffset.Offsets));
				writer.WriteStringValue(BasicOffsetConverter.ToString(value.Offsets));

				writer.WritePropertyName(nameof(FlagOffset.On));
				writer.WriteStringValue(ToString(value.On));

				writer.WritePropertyName(nameof(FlagOffset.Off));
				writer.WriteStringValue(ToString(value.Off));

				writer.WriteEndObject();
			}
		}

		public class BasicOffsetConverter : JsonConverter<Offset>
		{
			public static ulong[]? ToOffsets(string? str)
			{
				if (string.IsNullOrEmpty(str))
					return null;

				string[] parts = str.Split(',');
				List<ulong> values = new List<ulong>();
				foreach (string part in parts)
				{
					string strngVal = part.Trim(' ');

					if (!strngVal.StartsWith("0x"))
						throw new FormatException("Offset values must start with '0x'");

					values.Add(ulong.Parse(strngVal.Substring(2), NumberStyles.HexNumber));
				}

				return values.ToArray();
			}

			public static string ToString(ulong[] data)
			{
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < data.Length; i++)
				{
					if (i > 0 && i < data.Length)
						builder.Append(", ");

					builder.Append("0x");
					builder.Append(data[i].ToString("X2"));
				}

				return builder.ToString();
			}

			public override Offset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				string jsonStr = reader.GetString();
				Offset? offset = Activator.CreateInstance(typeToConvert, ToOffsets(jsonStr)) as Offset;

				if (offset == null)
					throw new Exception("Failed to create instance of offset");

				return offset;
			}

			public override void Write(Utf8JsonWriter writer, Offset value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(ToString(value.Offsets));
			}
		}
	}
}
