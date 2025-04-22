// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using Anamnesis.Files;
using Anamnesis.GameData.Excel;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a sheet of custom equipment data.
/// </summary>
public class EquipmentSheet : ISheet<Equipment>
{
	private readonly Dictionary<uint, Equipment> rows;

	/// <summary>
	/// Initializes a new instance of the <see cref="EquipmentSheet"/> class.
	/// </summary>
	/// <param name="fileName">The name of the file to load the equipment data from.</param>
	public EquipmentSheet(string fileName)
	{
		try
		{
			this.rows = [];
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

	/// <inheritdoc/>
	public bool Contains(uint key) => this.rows.ContainsKey(key);

	/// <inheritdoc/>
	public Equipment GetRow(uint key) => this.rows[key];

	/// <inheritdoc/>
	public Equipment GetRow(byte key) => this.rows[(uint)key];

	/// <inheritdoc/>
	public IEnumerator<Equipment> GetEnumerator()
	{
		return this.rows.Values.GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.rows.GetEnumerator();
	}
}
