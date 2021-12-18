// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Anamnesis.Files;

	public class ModelListSheet : IEnumerable<ModelListEntry>
	{
		private readonly Dictionary<uint, ModelListEntry> rows;

		public ModelListSheet(string fileName)
		{
			try
			{
				this.rows = new Dictionary<uint, ModelListEntry>();

				List<ModelListEntry> rows = EmbeddedFileUtility.Load<List<ModelListEntry>>(fileName);
				foreach (ModelListEntry accessory in rows)
				{
					this.rows.Add(accessory.ModelCharaRow, accessory);
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

		public ModelListEntry Get(uint key)
		{
			return this.rows[key];
		}

		public ModelListEntry Get(byte key)
		{
			return this.rows[(uint)key];
		}

		public IEnumerator<ModelListEntry> GetEnumerator()
		{
			return this.rows.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.rows.GetEnumerator();
		}
	}
}
