// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Collections.Generic;
	using Anamnesis.GameData;
	using Anamnesis.GameData.ViewModels;
	using Lumina.Excel;

	using LuminaData = Lumina.Lumina;

	public class Sheet<TInterfaceType, TConcreteType, TWrapperType> : IData<TInterfaceType>
		where TInterfaceType : IDataObject
		where TConcreteType : class, IExcelRow
		where TWrapperType : ExcelRowViewModel<TConcreteType>, TInterfaceType
	{
		private ExcelSheet<TConcreteType> excel;
		private Dictionary<int, TWrapperType> wrapperCache = new Dictionary<int, TWrapperType>();
		private List<TInterfaceType>? all;

		private LuminaData lumina;

		public Sheet(LuminaData lumina)
		{
			this.lumina = lumina;
			this.excel = lumina.GetExcelSheet<TConcreteType>();
		}

		public IEnumerable<TInterfaceType> All
		{
			get
			{
				if (this.all == null)
				{
					this.all = new List<TInterfaceType>();
					for (int i = 1; i < this.excel.RowCount; i++)
					{
						this.all.Add(this.Get(i));
					}
				}

				return this.all;
			}
		}

		public TInterfaceType Get(int key)
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

		public TInterfaceType Get(byte key)
		{
			return this.Get((int)key);
		}
	}
}
