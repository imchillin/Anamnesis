// © Anamnesis.
// Developed by W and A Walsh.
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

		private ExcelSheet<T> sheet;

		public ExcelRowViewModel(uint key, ExcelSheet<T> sheet, GameData lumina)
		{
			this.sheet = sheet;
			this.Key = key;
			this.lumina = lumina;

			try
			{
				T? row = this.sheet.GetRow(this.Key);

				if (row == null)
					throw new Exception($"No row found at {this.Key}");

				this.Value = row;
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to read Lumina row: {this.Key} for type: {typeof(T).Name}", ex);
			}
		}

		public uint Key
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
