// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using Lumina.Excel;
using System.Collections.Generic;
using System.Linq;

public static class ExcelSheetExtensions
{
	// TODO: See if its possible to make this emplicit so that we don't have to call it everywhere
	public static IEnumerable<object> ToEnumerable<T>(this ExcelSheet<T> sheet)
		where T : struct, IExcelRow<T>
	{
		return sheet.Cast<object>();
	}
}
