// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
	using Lumina;
	using Lumina.Excel;

	public abstract class ExcelRowViewModel<T> : IRow
		where T : class, IExcelRow
	{
		protected readonly Lumina lumina;

		private ExcelSheet<T> sheet;
		private T? value;

		public ExcelRowViewModel(int key, ExcelSheet<T> sheet, Lumina lumina)
		{
			this.sheet = sheet;
			this.Key = key;
			this.lumina = lumina;
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
				try
				{
					if (this.value == null)
						this.value = this.sheet.GetRow((uint)this.Key);

					return this.value;
				}
				catch (Exception ex)
				{
					Log.Write(ex);
					throw ex;
				}
			}
		}

		public abstract string Name
		{
			get;
		}

		public virtual string? Description
		{
			get => null;
		}
	}
}
