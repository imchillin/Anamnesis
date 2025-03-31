// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Memory;
using Anamnesis.Services;
using Lumina.Excel;
using System;

/// <summary>Represents a race in the game data.</summary>
[Sheet("Race", 0x3403807A)]
public readonly struct Race(ExcelPage page, uint offset, uint row)
	: IExcelRow<Race>
{
	/// <summary>Gets the row ID.</summary>
	public uint RowId => row;

	/// <summary>Gets the name of the race.</summary>
	public string Name => this.CustomizeRace.ToString();

	/// <summary>Gets the display name of the race.</summary>
	public string DisplayName => this.Masculine;

	/// <summary>Gets the corresponding actor customize race value.</summary>
	public ActorCustomizeMemory.Races CustomizeRace => (ActorCustomizeMemory.Races)this.RowId;

	/// <summary>Gets the masculine name of the race.</summary>
	public readonly string Masculine => page.ReadString(offset, offset).ToString();

	/// <summary>Gets the feminine name of the race.</summary>
	public readonly string Feminine => page.ReadString(offset + 4, offset).ToString();

	/// <summary>Gets the race-specific equipment (RSE) for the masculine body.</summary>
	public readonly RowRef<Item> RSEMBody => new(page.Module, (uint)page.ReadInt32(offset + 8), page.Language);

	/// <summary>Gets the race-specific equipment (RSE) for the feminine body.</summary>
	public readonly RowRef<Item> RSEFBody => new(page.Module, (uint)page.ReadInt32(offset + 12), page.Language);

	/// <summary>Gets the race-specific equipment (RSE) for the masculine hands.</summary>
	public readonly RowRef<Item> RSEMHands => new(page.Module, (uint)page.ReadInt32(offset + 16), page.Language);

	/// <summary>Gets the race-specific equipment (RSE) for the feminine hands.</summary>
	public readonly RowRef<Item> RSEFHands => new(page.Module, (uint)page.ReadInt32(offset + 20), page.Language);

	/// <summary>Gets the race-specific equipment (RSE) for the masculine legs.</summary>
	public readonly RowRef<Item> RSEMLegs => new(page.Module, (uint)page.ReadInt32(offset + 24), page.Language);

	/// <summary>Gets the race-specific equipment (RSE) for the feminine legs.</summary>
	public readonly RowRef<Item> RSEFLegs => new(page.Module, (uint)page.ReadInt32(offset + 28), page.Language);

	/// <summary>Gets the race-specific equipment (RSE) for the masculine feet.</summary>
	public readonly RowRef<Item> RSEMFeet => new(page.Module, (uint)page.ReadInt32(offset + 32), page.Language);

	/// <summary>Gets the race-specific equipment (RSE) for the feminine feet.</summary>
	public readonly RowRef<Item> RSEFFeet => new(page.Module, (uint)page.ReadInt32(offset + 36), page.Language);

	/// <summary>Gets the tribes associated with the race.</summary>
	/// <exception cref="Exception">Thrown when no tribes are found in game data or an unrecognized race is encountered.</exception>
	public Tribe[] Tribes
	{
		get
		{
			if (!Enum.IsDefined(this.CustomizeRace))
				return [];

			if (GameDataService.Tribes == null)
				throw new Exception("No tribes found in game data. Verify that Lumina has loaded this excel sheet.");

			return this.CustomizeRace switch
			{
				ActorCustomizeMemory.Races.Hyur =>
				[
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Midlander),
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Highlander),
				],
				ActorCustomizeMemory.Races.Elezen =>
				[
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Wildwood),
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Duskwight),
				],
				ActorCustomizeMemory.Races.Lalafel =>
				[
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Plainsfolk),
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Dunesfolk),
				],
				ActorCustomizeMemory.Races.Miqote =>
				[
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.SeekerOfTheSun),
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.KeeperOfTheMoon),
				],
				ActorCustomizeMemory.Races.Roegadyn =>
				[
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.SeaWolf),
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Hellsguard),
				],
				ActorCustomizeMemory.Races.AuRa =>
				[
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Raen),
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Xaela),
				],
				ActorCustomizeMemory.Races.Hrothgar =>
				[
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Helions),
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.TheLost),
				],
				ActorCustomizeMemory.Races.Viera =>
				[
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Rava),
					GameDataService.Tribes.GetRow((byte)ActorCustomizeMemory.Tribes.Veena),
				],
				_ => throw new Exception($"Unrecognized race {this.CustomizeRace}"),
			};
		}
	}

	/// <summary>
	/// Creates a new instance of the <see cref="Race"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Race"/> struct.</returns>
	static Race IExcelRow<Race>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
