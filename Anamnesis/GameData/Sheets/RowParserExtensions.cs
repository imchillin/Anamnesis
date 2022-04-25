// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using System;
using Anamnesis.Services;
using Lumina.Excel;
using Lumina.Text;

public static class RowParserExtensions
{
	public static ImageReference? ReadImageReference<TColumn>(this RowParser self, int column)
	{
		if (GameDataService.LuminaData == null)
			throw new Exception("Game Data Service is not initialized");

		TColumn? id = self.ReadColumn<TColumn>(column);

		if (id == null)
			return null;

		if (id is ushort uVal)
		{
			return new ImageReference(uVal);
		}
		else if (id is int iVal)
		{
			return new ImageReference(iVal);
		}
		else if (id is uint uiVal)
		{
			return new ImageReference(uiVal);
		}
		else
		{
			throw new Exception($"Unrecognised image reference column type: {typeof(TColumn)}");
		}
	}

	public static string? ReadString(this RowParser self, int column)
	{
		SeString? value = self.ReadColumn<SeString>(column);
		if (value == null)
			return null;

		if (string.IsNullOrEmpty(value.RawString) || string.IsNullOrWhiteSpace(value.RawString))
			return null;

		return value.RawString;
	}

	public static ushort ReadWeaponSet(this RowParser self, int column)
	{
		ulong val = self.ReadColumn<ulong>(column);
		return (ushort)val;
	}

	public static ushort ReadWeaponBase(this RowParser self, int column)
	{
		ulong val = self.ReadColumn<ulong>(column);
		return (ushort)(val >> 16);
	}

	public static ushort ReadWeaponVariant(this RowParser self, int column)
	{
		ulong val = self.ReadColumn<ulong>(column);
		return (ushort)(val >> 32);
	}

	public static ushort ReadSet(this RowParser self, int column)
	{
		return 0;
	}

	public static ushort ReadBase(this RowParser self, int column)
	{
		ulong val = self.ReadColumn<ulong>(column);
		return (ushort)val;
	}

	public static ushort ReadVariant(this RowParser self, int column)
	{
		ulong val = self.ReadColumn<ulong>(column);
		return (ushort)(val >> 16);
	}

	public static TRow? ReadRowReference<TColumn, TRow>(this RowParser self, int column, int minValue = int.MinValue)
		where TRow : Lumina.Excel.ExcelRow
	{
		TColumn? id = self.ReadColumn<TColumn>(column);

		if (id == null)
			throw new Exception($"Failed to read column: {column} as type: {typeof(TColumn)} for row reference.");

		ExcelSheet<TRow> sheet = GameDataService.GetSheet<TRow>();

		if (id is byte bVal)
		{
			return sheet.GetOrDefault((byte)Math.Max(bVal, minValue));
		}
		else if (id is uint uVal)
		{
			return sheet.GetOrDefault((uint)Math.Max(uVal, minValue));
		}
		else if (id is int iVal)
		{
			return sheet.GetOrDefault((uint)Math.Max(iVal, minValue));
		}
		else if (id is ushort sVal)
		{
			return sheet.GetOrDefault((ushort)Math.Max(sVal, minValue));
		}

		throw new Exception($"Unrecognized row reference key type: {typeof(TColumn)}");
	}
}
