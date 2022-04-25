// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using System;
using System.Collections;
using System.Collections.Generic;
using Anamnesis.Files;

public class EquipmentSheet : IEnumerable<Equipment>
{
	private readonly Dictionary<uint, Equipment> rows;

	public EquipmentSheet(string fileName)
	{
		try
		{
			this.rows = new();
			uint index = 0;
			var rows = EmbeddedFileUtility.Load<List<Equipment>>(fileName);

			foreach (Equipment equipment in rows)
			{
				this.rows.Add(index, equipment);
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

	public Equipment Get(uint key)
	{
		return this.rows[key];
	}

	public Equipment Get(byte key)
	{
		return this.rows[(uint)key];
	}

	public IEnumerator<Equipment> GetEnumerator()
	{
		return this.rows.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.rows.GetEnumerator();
	}
}
