// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Anamnesis.GameData;
	using Anamnesis.GameData.ViewModels;
	using Lumina.Excel;

	using LuminaData = Lumina.GameData;

	public class LuminaSheet<TInterfaceType, TConcreteType, TWrapperType> : ISheet<TInterfaceType>
		where TInterfaceType : IRow
		where TConcreteType : ExcelRow
		where TWrapperType : ExcelRowViewModel<TConcreteType>, TInterfaceType
	{
		private readonly ExcelSheet<TConcreteType> excel;
		private readonly Dictionary<uint, TWrapperType> wrapperCache = new Dictionary<uint, TWrapperType>();
		private readonly LuminaData lumina;

		public LuminaSheet(LuminaData lumina)
		{
			this.lumina = lumina;

			ExcelSheet<TConcreteType>? sheet = lumina.GetExcelSheet<TConcreteType>();

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
			lock (this.wrapperCache)
			{
				if (!this.wrapperCache.ContainsKey(key))
				{
					TWrapperType? wrapper = Activator.CreateInstance(typeof(TWrapperType), key, this.excel, this.lumina) as TWrapperType;

					if (wrapper == null)
						throw new Exception($"Failed to create instance of Lumina data wrapper: {typeof(TWrapperType)}");

					this.wrapperCache.Add(key, wrapper);
				}

				return this.wrapperCache[key];
			}
		}

		public TInterfaceType Get(byte key)
		{
			return this.Get((uint)key);
		}

		public IEnumerator<TInterfaceType> GetEnumerator()
		{
			for (uint i = 0; i < this.excel.RowCount; i++)
			{
				TInterfaceType value = this.Get(i);
				yield return value;
				continue;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			for (uint i = 0; i < this.excel.RowCount; i++)
			{
				TInterfaceType value = this.Get(i);
				yield return value;
				continue;
			}
		}
	}
}
