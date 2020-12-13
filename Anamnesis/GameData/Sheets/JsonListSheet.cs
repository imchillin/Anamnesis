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
		private Dictionary<int, T> rows;

		public JsonListSheet(string fileName)
		{
			try
			{
				List<T> rows = SerializerService.DeserializeFile<List<T>>(fileName);
				this.rows = new Dictionary<int, T>();

				int index = 0;
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
}
