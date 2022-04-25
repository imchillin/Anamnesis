// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Memory;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Tribe", 0xe74759fb)]
public class Tribe : ExcelRow
{
	public string Name => this.CustomizeTribe.ToString();
	public ActorCustomizeMemory.Tribes CustomizeTribe => (ActorCustomizeMemory.Tribes)this.RowId;

	public string Feminine { get; private set; } = string.Empty;
	public string Masculine { get; private set; } = string.Empty;

	public string DisplayName
	{
		get
		{
			// big old hack to keep miqo tribe names short for the UI
			if (this.Feminine.StartsWith("Seeker"))
				return "Seeker";

			if (this.Feminine.StartsWith("Keeper"))
				return "Keeper";

			return this.Feminine;
		}
	}

	public bool Equals(Tribe? other)
	{
		if (other is null)
			return false;

		return this.CustomizeTribe == other.CustomizeTribe;
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Masculine = parser.ReadColumn<SeString>(0) ?? string.Empty;
		this.Feminine = parser.ReadColumn<SeString>(1) ?? string.Empty;
	}
}
