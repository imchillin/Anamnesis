// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Anamnesis.Serialization;

	public class JsonListSheet<T> : ISheet<T>
		where T : IJsonRow
	{
		private Dictionary<uint, T> rows;

		public JsonListSheet(string fileName)
		{
			try
			{
				List<T> rows = SerializerService.DeserializeFile<List<T>>(fileName);
				this.rows = new Dictionary<uint, T>();

				uint index = 0;
				foreach (T value in rows)
				{
					value.Key = index;
					this.rows.Add(index, value);
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

		public T Get(uint key)
		{
			return this.rows[key];
		}

		public T Get(byte key)
		{
			return this.rows[(uint)key];
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
}
