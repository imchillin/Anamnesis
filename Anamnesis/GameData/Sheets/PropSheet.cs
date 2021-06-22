// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text.Json.Serialization;
	using Anamnesis.Character;
	using Anamnesis.Serialization;
	using Anamnesis.Serialization.Converters;

	public class PropSheet : ISheet<Prop>
	{
		private Dictionary<uint, Prop> rows;

		public PropSheet(string fileName)
		{
			try
			{
				this.rows = new Dictionary<uint, Prop>();

				uint index = 0;
				Dictionary<string, string> stringRows = SerializerService.DeserializeFile<Dictionary<string, string>>(fileName);
				foreach ((string key, string value) in stringRows)
				{
					string[] parts = value.Split(';', StringSplitOptions.RemoveEmptyEntries);

					(ushort modelSet, ushort modelBase, ushort modelVariant) = IItemConverter.SplitString(key);
					Prop prop = new Prop();
					prop.Name = parts[0].Trim();

					if (parts.Length == 2)
						prop.Description = parts[1].Trim();

					prop.ModelBase = modelBase;
					prop.ModelVariant = modelVariant;
					prop.ModelSet = modelSet;

					this.rows.Add(index, prop);

					index++;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to load json data: {fileName}", ex);
			}
		}

		public bool Contains(uint key)
		{
			return this.rows.ContainsKey(key);
		}

		public Prop Get(uint key)
		{
			return this.rows[key];
		}

		public Prop Get(byte key)
		{
			return this.rows[(uint)key];
		}

		public IEnumerator<Prop> GetEnumerator()
		{
			return this.rows.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.rows.GetEnumerator();
		}
	}

	#pragma warning disable SA1201
	public interface IJsonRow : IRow
	{
		public new uint Key { get; set; }
	}
}
