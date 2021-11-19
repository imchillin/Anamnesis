// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
	using Lumina;
	using Lumina.Excel;

	public abstract class ExcelRowViewModel<T> : IRow
		where T : ExcelRow
	{
		protected readonly GameData lumina;

		private readonly ExcelSheet<T> sheet;

		public ExcelRowViewModel(uint key, ExcelSheet<T> sheet, GameData lumina)
		{
			this.sheet = sheet;
			this.RowId = key;
			this.lumina = lumina;

			try
			{
				T? row = this.sheet.GetRow(this.RowId);

				if (row == null)
					throw new Exception($"No row found at {this.RowId}");

				this.Value = row;
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to read Lumina row: {this.RowId} for type: {typeof(T).Name}", ex);
			}
		}

		public uint RowId
		{
			get;
			private set;
		}

		public T Value { get; private set; }

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
