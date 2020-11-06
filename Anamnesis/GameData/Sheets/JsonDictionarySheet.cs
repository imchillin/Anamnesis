// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Anamnesis.Serialization;

	public class JsonDictionarySheet<T> : ISheet<T>
		where T : IJsonRow
	{
		private Dictionary<int, T> rows;

		public JsonDictionarySheet(string fileName)
		{
			try
			{
				Dictionary<string, T> stringRows = SerializerService.DeserializeFile<Dictionary<string, T>>(fileName);
				this.rows = new Dictionary<int, T>();

				foreach ((string key, T value) in stringRows)
				{
					int intKey = int.Parse(key);
					value.Key = intKey;
					this.rows.Add(intKey, value);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to load json data: {fileName}", ex);
			}
		}

		public bool Contains(int key)
		{
			return this.rows.ContainsKey(key);
		}

		public T Get(int key)
		{
			return this.rows[key];
		}

		public T Get(byte key)
		{
			return this.rows[(int)key];
		}

		public IEnumerator<T> GetEnumerator()
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
		public new int Key { get; set; }
	}
}
