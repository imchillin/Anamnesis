// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using Anamnesis.GameData;
	using Anamnesis.GameData.ViewModels;
	using Lumina.Excel;
	using Serilog;
	using LuminaData = Lumina.GameData;

	public class LuminaSheet<TInterfaceType, TConcreteType, TWrapperType> : ISheet<TInterfaceType>
		where TInterfaceType : IRow
		where TConcreteType : ExcelRow
		where TWrapperType : ExcelRowViewModel<TConcreteType>, TInterfaceType
	{
		private readonly Lumina.Excel.ExcelSheet<TConcreteType> excel;
		private readonly ConcurrentDictionary<uint, TWrapperType> wrapperCache = new ConcurrentDictionary<uint, TWrapperType>();
		private readonly LuminaData lumina;

		public LuminaSheet(LuminaData lumina)
		{
			this.lumina = lumina;

			Lumina.Excel.ExcelSheet<TConcreteType>? sheet = lumina.GetExcelSheet<TConcreteType>();

			if (sheet == null)
				throw new Exception($"Failed to read lumina excel sheet: {typeof(TConcreteType)}");

			this.excel = sheet;
		}

		public bool Contains(uint key)
		{
			lock (this.excel)
			{
				TConcreteType? row = this.excel.GetRow((uint)key);
				return row != null;
			}
		}

		public TInterfaceType Get(uint key)
		{
			if (!this.wrapperCache.ContainsKey(key))
			{
				TWrapperType? wrapper;

				try
				{
					wrapper = Activator.CreateInstance(typeof(TWrapperType), key, this.excel, this.lumina) as TWrapperType;
				}
				catch (Exception ex)
				{
					throw new Exception($"Failed to create instance of Lumina data wrapper: {typeof(TWrapperType)}", ex);
				}

				if (wrapper == null)
					throw new Exception($"Failed to create instance of Lumina data wrapper: {typeof(TWrapperType)}");

				this.wrapperCache.TryAdd(key, wrapper);
			}

			return this.wrapperCache[key];
		}

		public TInterfaceType Get(byte key)
		{
			return this.Get((uint)key);
		}

		public IEnumerator<TInterfaceType> GetEnumerator()
		{
			lock (this.excel)
			{
				foreach (TConcreteType concrete in this.excel)
				{
					TInterfaceType value = this.Get(concrete.RowId);
					yield return value;
					continue;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
