// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Anamnesis.Files;
	using Anamnesis.Serialization;

	public class JsonDictionarySheet<TValue, TWrapper> : ISheet<TWrapper>
		where TWrapper : JsonDictionarySheet<TValue, TWrapper>.Entry, new()
	{
		private readonly Dictionary<uint, TWrapper> rows = new Dictionary<uint, TWrapper>();

		public JsonDictionarySheet(string fileName)
		{
			try
			{
				Dictionary<uint, TValue>? data = EmbeddedFileUtility.Load<Dictionary<uint, TValue>>(fileName);

				foreach ((uint key, TValue value) in data)
				{
					TWrapper entry = new TWrapper();
					entry.Key = key;
					entry.SetValue(value);
					this.rows.Add(key, entry);
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

		public TWrapper Get(uint key)
		{
			if (!this.rows.ContainsKey(key))
				throw new Exception($"Key not found: {key}");

			return this.rows[key];
		}

		public TWrapper Get(byte key)
		{
			return this.Get((uint)key);
		}

		public IEnumerator<TWrapper> GetEnumerator()
		{
			return this.rows.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.rows.Values.GetEnumerator();
		}

		public class Entry : IRow
		{
			public uint Key { get; set; }
			public string Name => string.Empty;
			public string? Description => null;
			public TValue? Value { get; private set; }

			public virtual void SetValue(TValue value)
			{
				this.Value = value;
			}
		}
	}
}
