// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using System;

public class ExcelRow : Lumina.Excel.ExcelRow, IEquatable<ExcelRow>
{
	public static bool operator ==(ExcelRow? lhs, ExcelRow? rhs)
	{
		if (ReferenceEquals(lhs, rhs))
			return true;

		if (ReferenceEquals(lhs, null))
			return false;

		if (ReferenceEquals(rhs, null))
			return false;

		return lhs.Equals(rhs);
	}

	public static bool operator !=(ExcelRow? lhs, ExcelRow? rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(ExcelRow? other)
	{
		if (ReferenceEquals(other, null))
			return false;

		if (ReferenceEquals(this, other))
			return true;

		return this.RowId.Equals(other.RowId)
			   && this.SubRowId.Equals(other.SubRowId)
			   && this.SheetName.Equals(other.SheetName)
			   && this.SheetLanguage.Equals(other.SheetLanguage);
	}

	public override bool Equals(object? obj)
	{
		return this.Equals(obj as ExcelRow);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.RowId, this.SubRowId, this.SheetName, this.SheetLanguage);
	}
}
