// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Wrappers
{
	using Lumina.Excel;

	public abstract class ExcelRowWrapper<T> : IDataObject
		where T : class, IExcelRow
	{
		private ExcelSheet<T> sheet;
		private T? value;

		public ExcelRowWrapper(int key, ExcelSheet<T> sheet)
		{
			this.sheet = sheet;
			this.Key = key;
		}

		public int Key
		{
			get;
			private set;
		}

		public T Value
		{
			get
			{
				if (this.value == null)
					this.value = this.sheet.GetRow((uint)this.Key);

				return this.value;
			}
		}
	}
}
