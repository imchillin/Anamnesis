// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Memory;
using Lumina.Excel;

[Sheet("EquipRaceCategory", 0xF914B198)]
public readonly struct EquipRaceCategory(ExcelPage page, uint offset, uint row)
	: IExcelRow<EquipRaceCategory>
{
	public uint RowId => row;

	public readonly bool Hyur => page.ReadBool(offset);
	public readonly bool Elezen => page.ReadBool(offset + 1);
	public readonly bool Lalafell => page.ReadBool(offset + 2);
	public readonly bool Miqote => page.ReadBool(offset + 3);
	public readonly bool Roegadyn => page.ReadBool(offset + 4);
	public readonly bool AuRa => page.ReadBool(offset + 5);
	public bool Hrothgar => page.ReadBool(offset + 6);
	public bool Viera => page.ReadBool(offset + 7);
	public readonly bool Male => page.ReadPackedBool(offset + 8, 0);
	public readonly bool Female => page.ReadPackedBool(offset + 8, 1);

	static EquipRaceCategory IExcelRow<EquipRaceCategory>.Create(ExcelPage page, uint offset, uint row) =>
	   new(page, offset, row);

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
