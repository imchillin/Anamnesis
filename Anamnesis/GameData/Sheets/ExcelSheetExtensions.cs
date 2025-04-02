// © Anamnesis.
// Licensed under the MIT license.

using Lumina.Excel;
using System.Collections.Generic;
using System.Linq;

namespace Anamnesis.GameData.Sheets;

public static class ExcelSheetExtensions
{
	/// <summary>
	/// Converts an Excel sheet to an object IEnumerable collection.
	/// </summary>
	/// <typeparam name="T">The type of the sheet.</typeparam>
	/// <param name="sheet">The sheet to convert.</param>
	/// <returns>The sheet as an IEnumerable collection.</returns>
	public static IEnumerable<object> ToEnumerable<T>(this ExcelSheet<T> sheet)
		where T : struct, IExcelRow<T>
	{
		return sheet.Cast<object>();
	}
}
