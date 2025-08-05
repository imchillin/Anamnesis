// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents the class job category data often associated with items in the game.
/// </summary>
[Sheet("ClassJobCategory", 0xFB018ADA)]
public readonly struct ClassJobCategory(ExcelPage page, uint offset, uint row)
	: IExcelRow<ClassJobCategory>
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>
	/// Gets the singular name of the class job category.
	/// </summary>
	public readonly string Name => page.ReadString(offset, offset).ToString();

	/// <summary>
	/// Gets the class jobs supported by this category.
	/// </summary>
	public Dictionary<Classes, bool> ClassJobs { get; } = new Dictionary<Classes, bool>
	{
		{ Classes.Gladiator, page.ReadBool(offset + 5) },
		{ Classes.Pugilist, page.ReadBool(offset + 6) },
		{ Classes.Marauder, page.ReadBool(offset + 7) },
		{ Classes.Lancer, page.ReadBool(offset + 8) },
		{ Classes.Archer, page.ReadBool(offset + 9) },
		{ Classes.Conjurer, page.ReadBool(offset + 10) },
		{ Classes.Thaumaturge, page.ReadBool(offset + 11) },
		{ Classes.Carpenter, page.ReadBool(offset + 12) },
		{ Classes.Blacksmith, page.ReadBool(offset + 13) },
		{ Classes.Armorer, page.ReadBool(offset + 14) },
		{ Classes.Goldsmith, page.ReadBool(offset + 15) },
		{ Classes.Leatherworker, page.ReadBool(offset + 16) },
		{ Classes.Weaver, page.ReadBool(offset + 17) },
		{ Classes.Alchemist, page.ReadBool(offset + 18) },
		{ Classes.Culinarian, page.ReadBool(offset + 19) },
		{ Classes.Miner, page.ReadBool(offset + 20) },
		{ Classes.Botanist, page.ReadBool(offset + 21) },
		{ Classes.Fisher, page.ReadBool(offset + 22) },
		{ Classes.Paladin, page.ReadBool(offset + 23) },
		{ Classes.Monk, page.ReadBool(offset + 24) },
		{ Classes.Warrior, page.ReadBool(offset + 25) },
		{ Classes.Dragoon, page.ReadBool(offset + 26) },
		{ Classes.Bard, page.ReadBool(offset + 27) },
		{ Classes.WhiteMage, page.ReadBool(offset + 28) },
		{ Classes.BlackMage, page.ReadBool(offset + 29) },
		{ Classes.Arcanist, page.ReadBool(offset + 30) },
		{ Classes.Summoner, page.ReadBool(offset + 31) },
		{ Classes.Scholar, page.ReadBool(offset + 32) },
		{ Classes.Rogue, page.ReadBool(offset + 33) },
		{ Classes.Ninja, page.ReadBool(offset + 34) },
		{ Classes.Machinist, page.ReadBool(offset + 35) },
		{ Classes.DarkKnight, page.ReadBool(offset + 36) },
		{ Classes.Astrologian, page.ReadBool(offset + 37) },
		{ Classes.Samurai, page.ReadBool(offset + 38) },
		{ Classes.RedMage, page.ReadBool(offset + 39) },
		{ Classes.BlueMage, page.ReadBool(offset + 40) },
		{ Classes.Gunbreaker, page.ReadBool(offset + 41) },
		{ Classes.Dancer, page.ReadBool(offset + 42) },
		{ Classes.Reaper, page.ReadBool(offset + 43) },
		{ Classes.Sage, page.ReadBool(offset + 44) },
		{ Classes.Viper, page.ReadBool(offset + 45) },
		{ Classes.Pictomancer, page.ReadBool(offset + 46) },
	};

	/// <summary>
	/// Creates a new instance of the <see cref="ClassJobCategory"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="ClassJobCategory"/> struct.</returns>
	static ClassJobCategory IExcelRow<ClassJobCategory>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	/// <summary>
	/// Checks if the class job category contains the specified class job.
	/// </summary>
	/// <param name="classJob">The class job to check for.</param>
	/// <returns>
	/// True if the class job category contains the specified class job, otherwise false.
	/// </returns>
	public bool Contains(Classes classJob)
	{
		if (this.ClassJobs.TryGetValue(classJob, out bool value))
			return value;

		return false;
	}

	/// <summary>
	/// Converts the class job category to a <see cref="Classes"/> enum.
	/// </summary>
	/// <returns>The class job category as a <see cref="Classes"/> enum.</returns>
	public Classes ToFlags()
	{
		Classes classes = Classes.None;

		foreach (Classes? job in Enum.GetValues<Classes>().Select(v => (Classes?)v))
		{
			if (job == null || job == Classes.None || job == Classes.All)
				continue;

			if (this.Contains((Classes)job))
			{
				classes |= (Classes)job;
			}
		}

		return classes;
	}
}
