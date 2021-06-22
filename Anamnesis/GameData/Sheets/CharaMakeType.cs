// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using Lumina.Data;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	using LuminaData = Lumina.GameData;

	[Sheet("CharaMakeType", columnHash: 0x80d7db6d)]
	public class CharaMakeType : ExcelRow
	{
		public LazyRow<Race>? Race;
		public LazyRow<Tribe>? Tribe;
		public sbyte Gender;
		public int[]? FacialFeatureOptions;

		public override void PopulateData(RowParser parser, LuminaData lumina, Language language)
		{
			this.RowId = parser.Row;
			this.SubRowId = parser.SubRow;

			this.Race = new LazyRow<Race>(lumina, parser.ReadColumn<int>(0), language);
			this.Tribe = new LazyRow<Tribe>(lumina, parser.ReadColumn<int>(1), language);
			this.Gender = parser.ReadColumn<sbyte>(2);

			this.FacialFeatureOptions = new int[7 * 8];
			for (int i = 0; i < 7 * 8; i++)
			{
				this.FacialFeatureOptions[i] = parser.ReadColumn<int>(3291 + i);
			}
		}
	}
}