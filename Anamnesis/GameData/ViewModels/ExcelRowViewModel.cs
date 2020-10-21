// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Lumina.Excel;

	public abstract class ExcelRowViewModel<T> : IDataObject
		where T : class, IExcelRow
	{
		private ExcelSheet<T> sheet;
		private T? value;

		public ExcelRowViewModel(int key, ExcelSheet<T> sheet)
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
