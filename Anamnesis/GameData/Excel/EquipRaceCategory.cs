// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Memory;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("EquipRaceCategory", 0xf914b198)]
public class EquipRaceCategory : ExcelRow
{
	public bool Hyur { get; private set; }
	public bool Elezen { get; private set; }
	public bool Lalafell { get; private set; }
	public bool Miqote { get; private set; }
	public bool Roegadyn { get; private set; }
	public bool AuRa { get; private set; }
	public bool Hrothgar { get; private set; }
	public bool Viera { get; private set; }
	public bool Male { get; private set; }
	public bool Female { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.Hyur = parser.ReadColumn<bool>(0);
		this.Elezen = parser.ReadColumn<bool>(1);
		this.Lalafell = parser.ReadColumn<bool>(2);
		this.Miqote = parser.ReadColumn<bool>(3);
		this.Roegadyn = parser.ReadColumn<bool>(4);
		this.AuRa = parser.ReadColumn<bool>(5);
		this.Hrothgar = parser.ReadColumn<bool>(6);
		this.Viera = parser.ReadColumn<bool>(7);
		this.Male = parser.ReadColumn<bool>(8);
		this.Female = parser.ReadColumn<bool>(9);
	}

	public bool CanEquip(ActorCustomizeMemory.Races race, ActorCustomizeMemory.Genders gender)
	{
		if (!this.Male && gender == ActorCustomizeMemory.Genders.Masculine)
			return false;

		if (!this.Female && gender == ActorCustomizeMemory.Genders.Feminine)
			return false;

		if (!this.Hyur && race == ActorCustomizeMemory.Races.Hyur)
			return false;

		if (!this.Elezen && race == ActorCustomizeMemory.Races.Elezen)
			return false;

		if (!this.Lalafell && race == ActorCustomizeMemory.Races.Lalafel)
			return false;

		if (!this.Miqote && race == ActorCustomizeMemory.Races.Miqote)
			return false;

		if (!this.Roegadyn && race == ActorCustomizeMemory.Races.Roegadyn)
			return false;

		if (!this.AuRa && race == ActorCustomizeMemory.Races.AuRa)
			return false;

		if (!this.Hrothgar && race == ActorCustomizeMemory.Races.Hrothgar)
			return false;

		if (!this.Viera && race == ActorCustomizeMemory.Races.Viera)
			return false;

		return true;
	}
}
