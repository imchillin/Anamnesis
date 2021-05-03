// © Anamnesis.
// Developed by W and A Walsh.
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
		private ExcelSheet<TConcreteType> excel;
		private Dictionary<int, TWrapperType> wrapperCache = new Dictionary<int, TWrapperType>();
		private List<TInterfaceType>? all;

		private LuminaData lumina;

		public LuminaSheet(LuminaData lumina)
		{
			this.lumina = lumina;
			this.excel = lumina.GetExcelSheet<TConcreteType>();
		}

		private IEnumerable<TInterfaceType> All
		{
			get
			{
				if (this.all == null)
				{
					this.all = new List<TInterfaceType>();

					foreach (TConcreteType? entry in this.excel)
					{
						TInterfaceType viewModel = this.Get((int)entry.RowId);
						this.all.Add(viewModel);
					}

					/*for (int i = 1; i < this.excel.RowCount; i++)
					{
						TInterfaceType entry = this.Get(i);

						if (!entry.IsValid)
							continue;

						this.all.Add(entry);
					}*/
				}

				return this.all;
			}
		}

		public bool Contains(int key)
		{
			TConcreteType? row = this.excel.GetRow((uint)key);
			return row != null;
		}

		public TInterfaceType Get(int key)
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
			return this.Get((int)key);
		}

		public IEnumerator<TInterfaceType> GetEnumerator()
		{
			return this.All.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.All.GetEnumerator();
		}
	}
}
