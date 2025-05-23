﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Memory;
using Lumina.Excel;

/// <summary>
/// Represents the race category data associated with game items.
/// </summary>
[Sheet("EquipRaceCategory", 0xF914B198)]
public readonly struct EquipRaceCategory(ExcelPage page, uint offset, uint row)
	: IExcelRow<EquipRaceCategory>
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>
	/// Gets a value whether the item can be equipped by Hyur characters.
	/// </summary>
	public readonly bool Hyur => page.ReadBool(offset);

	/// <summary>
	/// Gets a value whether the item can be equipped by Elezen characters.
	/// </summary>
	public readonly bool Elezen => page.ReadBool(offset + 1);

	/// <summary>
	/// Gets a value whether the item can be equipped by Lalafell characters.
	/// </summary>
	public readonly bool Lalafell => page.ReadBool(offset + 2);

	/// <summary>
	/// Gets a value whether the item can be equipped by Miqo'te characters.
	/// </summary>
	public readonly bool Miqote => page.ReadBool(offset + 3);

	/// <summary>
	/// Gets a value whether the item can be equipped by Roegadyn characters.
	/// </summary>
	public readonly bool Roegadyn => page.ReadBool(offset + 4);

	/// <summary>
	/// Gets a value whether the item can be equipped by Au Ra characters.
	/// </summary>
	public readonly bool AuRa => page.ReadBool(offset + 5);

	/// <summary>
	/// Gets a value whether the item can be equipped by Hrothgar characters.
	/// </summary>
	public bool Hrothgar => page.ReadBool(offset + 6);

	/// <summary>
	/// Gets a value whether the item can be equipped by Viera characters.
	/// </summary>
	public bool Viera => page.ReadBool(offset + 7);

	/// <summary>
	/// Gets a value whether the item can be equipped by male characters.
	/// </summary>
	public readonly bool Male => page.ReadPackedBool(offset + 8, 0);

	/// <summary>
	/// Gets a value whether the item can be equipped by female characters.
	/// </summary>
	public readonly bool Female => page.ReadPackedBool(offset + 8, 1);

	/// <summary>
	/// Creates a new instance of the <see cref="EquipRaceCategory"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="EquipRaceCategory"/> struct.</returns>
	static EquipRaceCategory IExcelRow<EquipRaceCategory>.Create(ExcelPage page, uint offset, uint row) =>
	   new(page, offset, row);

	/// <summary>
	/// Checks if the provided race and gender can equip this item.
	/// </summary>
	/// <param name="race">The playable race.</param>
	/// <param name="gender">The gender.</param>
	/// <returns>True if the item can be equipped, false otherwise.</returns>
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
