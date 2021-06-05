// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters
{
	using System;
	using System.Text.Json;
	using System.Text.Json.Serialization;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;
	using Anamnesis.Services;

	public class IItemConverter : JsonConverter<IItem>
	{
		public static (ushort modelSet, ushort modelBase, ushort modelVariant) SplitString(string str)
		{
			string[] parts = str.Split(",", StringSplitOptions.RemoveEmptyEntries);
			ushort modelSet = ushort.Parse(parts[0].Trim());
			ushort modelBase = ushort.Parse(parts[1].Trim());
			ushort modelVariant = ushort.Parse(parts[2].Trim());

			return (modelSet, modelBase, modelVariant);
		}

		public static IItem FromString(string str)
		{
			if (str.Contains(","))
			{
				(ushort modelSet, ushort modelBase, ushort modelVariant) = SplitString(str);
				return ItemUtility.GetDummyItem(modelSet, modelBase, modelVariant);
			}
			else
			{
				return GameDataService.Items.Get(uint.Parse(str));
			}
		}

		public override IItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			string? str = reader.GetString();

			if (str == null)
				throw new Exception($"Invalid serialized item value");

			return FromString(str);
		}

		public override void Write(Utf8JsonWriter writer, IItem value, JsonSerializerOptions options)
		{
			if (value.Key != 0)
			{
				writer.WriteStringValue($"{value.Key}");
			}
			else
			{
				writer.WriteStringValue($"{value.ModelSet}, {value.ModelBase}, {value.ModelVariant}");
			}
		}
	}
}
