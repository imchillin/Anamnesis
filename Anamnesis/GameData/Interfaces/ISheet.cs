// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using System.Collections.Generic;

public interface ISheet<T> : IEnumerable<T>
	where T : IRow
{
	/// <summary>
	/// Determines whether the sheet contains a row entry with the specified row identifier.
	/// </summary>
	/// <param name="rowId">The row identifier.</param>
	/// <returns>True if the sheet contains the entry; otherwise, false.</returns>
	bool Contains(uint rowId);

	/// <summary>
	/// Gets the entry from the sheet with the specified row identifier.
	/// </summary>
	/// <param name="rowId">The row identifier.</param>
	/// <returns>The row entry.</returns>
	T GetRow(uint rowId);

	/// <summary>
	/// Gets the row entry from the sheet with the specified row identifier.
	/// </summary>
	/// <param name="rowId">The row identifier.</param>
	/// <returns>The row entry.</returns>
	T GetRow(byte rowId);
}
