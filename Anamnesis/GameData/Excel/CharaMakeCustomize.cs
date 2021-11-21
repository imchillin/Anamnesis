// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel
{
	using System.Windows.Media;
	using Anamnesis.GameData.Sheets;
	using Lumina.Data;
	using Lumina.Excel;

	[Sheet("CharaMakeCustomize", 732348175u)]
	public class CharaMakeCustomize : ExcelRow
	{
		public string Name { get; set; } = string.Empty;
		public ImageSource? Icon { get; private set; }
		public ImageSource? ItemIcon { get; private set; }
		public byte FeatureId { get; private set; }

		public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
		{
			base.PopulateData(parser, gameData, language);
			this.FeatureId = parser.ReadColumn<byte>(0);
			this.Icon = parser.ReadImageReference<uint>(1);

			Item? item = parser.ReadRowReference<uint, Item>(5);
			if (item != null && item.RowId != 0)
			{
				this.Name = item.Name;
				this.ItemIcon = item.Icon;
			}
			else
			{
				this.Name = $"Feature #{this.RowId}";
			}
		}
	}
}
