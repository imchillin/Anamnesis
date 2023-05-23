// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lumina.Data;
using Lumina.Data.Files.Excel;
using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using Lumina.Excel.Exceptions;
using Serilog;

public class ExcelSheet<T> : Lumina.Excel.ExcelSheet<T>, IEnumerable<T>, IEnumerable
	where T : Lumina.Excel.ExcelRow
{
	private readonly Lumina.GameData gameData;

	private readonly ConcurrentDictionary<ulong, T> cache = new ConcurrentDictionary<ulong, T>();

	public ExcelSheet(ExcelHeaderFile headerFile, string name, Language requestedLanguage, Lumina.GameData gameData)
		: base(headerFile, name, requestedLanguage, gameData)
	{
		this.gameData = gameData;
	}

	public static ExcelSheet<T> GetSheet(Lumina.GameData lumina)
	{
		SheetAttribute? attr = typeof(T).GetCustomAttribute<SheetAttribute>();

		if (attr == null)
			throw new Exception($"Missing SheetAttribute on type: {typeof(T)}");

		return GetSheet(lumina, attr.Name, lumina.Options.DefaultExcelLanguage, attr.ColumnHash);
	}

	public T Get(byte row)
	{
		return this.Get((uint)row);
	}

	public T Get(uint row, uint subRow = uint.MaxValue)
	{
		T? value = this.GetRowInternal(row, subRow);

		if (value == null)
			throw new Exception($"No row: {row} in sheet {this.GetType()}");

		return value;
	}

	public T? GetOrDefault(uint row, uint subRow = uint.MaxValue)
	{
		return this.GetRowInternal(row, subRow);
	}

	// Copies almost verbatum from /Lumina/src/Lumina/Excel/ExcelSheet.cs#L80
	// but with the row cache removed.
	public new IEnumerator<T> GetEnumerator()
	{
		foreach (ExcelPage? page in this.DataPages)
		{
			ExcelDataFile? file = page.File;
			Dictionary<uint, Lumina.Data.Structs.Excel.ExcelDataOffset>? rowPtrs = file.RowData;

			RowParser? parser = new RowParser(this, file);

			foreach (Lumina.Data.Structs.Excel.ExcelDataOffset rowPtr in rowPtrs.Values)
			{
				if (this.Header.Variant == ExcelVariant.Subrows)
				{
					// required to read the row header out and know how many subrows there is
					parser.SeekToRow(rowPtr.RowId);

					// read subrows
					for (uint i = 0; i < parser.RowCount; i++)
					{
						T? value = this.GetRowInternal(rowPtr.RowId, i);

						if (value == null)
							continue;

						yield return value;
						continue;
					}
				}
				else
				{
					T? value = this.GetRowInternal(rowPtr.RowId);

					if (value == null)
						continue;

					yield return value;
					continue;
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	// Copied almost verbatum from /Lumina/src/Lumina/Excel/ExcelModule.cs#L203
	// but with the sheet cache removed.
	private static ExcelSheet<T> GetSheet(Lumina.GameData lumina, string name, Language language, uint? expectedHash)
	{
		Log.Debug("sheet {SheetName} not in cache - creating new sheet for language {Language}", name, language);
		string path = ExcelModule.BuildExcelHeaderPath(name);
		ExcelHeaderFile? file = lumina.GetFile<ExcelHeaderFile>(path);

		if (file == null)
			throw new Exception($"Failed to get data file: {path}");

		if (expectedHash.HasValue)
		{
			uint columnsHash = file.GetColumnsHash();
			if (columnsHash != expectedHash)
			{
				Log.Warning($"The sheet {typeof(T).FullName} hash doesn't match the hash generated from the header. Expected: {expectedHash} actual: {columnsHash}");

				if (lumina.Options.PanicOnSheetChecksumMismatch)
				{
					throw new ExcelSheetColumnChecksumMismatchException(name, expectedHash.Value, columnsHash);
				}
			}
		}

		ExcelSheet<T>? excelSheet = Activator.CreateInstance(typeof(ExcelSheet<T>), file, name, language, lumina) as ExcelSheet<T>;

		if (excelSheet == null)
			throw new Exception("Failed to create excel sheet");

		excelSheet.GenerateFilePages();
		return excelSheet;
	}

	// Copied almost verbatum from /Lumina/src/Lumina/Excel/ExcelSheetImpl.cs#L119
	// since its internal...
	private void GenerateFilePages()
	{
		Language lang = Language.None;

		if (this.HeaderFile.Languages.Contains(this.RequestedLanguage))
			lang = this.RequestedLanguage;

		foreach (Lumina.Data.Structs.Excel.ExcelDataPagination bp in this.HeaderFile.DataPages)
		{
			string? filePath = this.GenerateFilePath(this.Name, bp.StartId, lang);

			// ignore languages that don't exist in this client build
			if (!this.gameData.FileExists(filePath))
				continue;

			ExcelPage segment = new ExcelPage();
			segment.FilePath = filePath;
			segment.RowCount = bp.RowCount;
			segment.StartId = bp.StartId;

			ExcelDataFile? file = this.gameData.GetFile<ExcelDataFile>(segment.FilePath);

			if (file == null)
				throw new Exception($"Failed to get segment file: {segment.FilePath}");

			segment.File = file;

			// convert big endian to little endian on le systems
			////this.ProcessDataEndianness(segment.File);

			this.DataPages.Add(segment);
		}
	}

	private T? GetRowInternal(uint row, uint subRow = uint.MaxValue)
	{
		try
		{
			ulong cacheKey = ExcelSheetImpl.GetCacheKey(row, subRow);

			T? value;
			if (this.cache.TryGetValue(cacheKey, out value))
				return value;

			// Lumina does not support concurrent access to the rowParser
			// or underlying FileResource.Stream, so make sure we dont concurrently
			// load rows.
			lock (this)
			{
				RowParser? rowParser = this.GetRowParser(row);

				if (rowParser == null)
					return null;

				T val = Activator.CreateInstance<T>();

				val.PopulateData(rowParser, this.gameData, this.RequestedLanguage);

				this.cache.TryAdd(cacheKey, val);

				return val;
			}
		}
		catch (Exception ex)
		{
			Log.Verbose(ex.Message);
			throw;
		}
	}
}
