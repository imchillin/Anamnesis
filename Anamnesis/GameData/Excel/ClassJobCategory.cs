// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using System;
using System.Collections.Generic;
using Anamnesis.GameData.Sheets;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("ClassJobCategory", 0x65bbdb12)]
public class ClassJobCategory : ExcelRow
{
	public string? Name { get; set; }

	public Dictionary<Classes, bool> ClassJobs { get; set; } = new Dictionary<Classes, bool>();

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.Name = parser.ReadString(0);

		////ADV = ((parser.ReadColumn<bool>(1) ? ((byte)1) : ((byte)0)) != 0);

		this.ClassJobs.Add(Classes.Gladiator, parser.ReadColumn<bool>(2));
		this.ClassJobs.Add(Classes.Pugilist, parser.ReadColumn<bool>(3));
		this.ClassJobs.Add(Classes.Marauder, parser.ReadColumn<bool>(4));
		this.ClassJobs.Add(Classes.Lancer, parser.ReadColumn<bool>(5));
		this.ClassJobs.Add(Classes.Archer, parser.ReadColumn<bool>(6));
		this.ClassJobs.Add(Classes.Conjurer, parser.ReadColumn<bool>(7));
		this.ClassJobs.Add(Classes.Thaumaturge, parser.ReadColumn<bool>(8));
		this.ClassJobs.Add(Classes.Carpenter, parser.ReadColumn<bool>(9));
		this.ClassJobs.Add(Classes.Blacksmith, parser.ReadColumn<bool>(10));
		this.ClassJobs.Add(Classes.Armorer, parser.ReadColumn<bool>(11));
		this.ClassJobs.Add(Classes.Goldsmith, parser.ReadColumn<bool>(12));
		this.ClassJobs.Add(Classes.Leatherworker, parser.ReadColumn<bool>(13));
		this.ClassJobs.Add(Classes.Weaver, parser.ReadColumn<bool>(14));
		this.ClassJobs.Add(Classes.Alchemist, parser.ReadColumn<bool>(15));
		this.ClassJobs.Add(Classes.Culinarian, parser.ReadColumn<bool>(16));
		this.ClassJobs.Add(Classes.Miner, parser.ReadColumn<bool>(17));
		this.ClassJobs.Add(Classes.Botanist, parser.ReadColumn<bool>(18));
		this.ClassJobs.Add(Classes.Fisher, parser.ReadColumn<bool>(19));
		this.ClassJobs.Add(Classes.Paladin, parser.ReadColumn<bool>(20));
		this.ClassJobs.Add(Classes.Monk, parser.ReadColumn<bool>(21));
		this.ClassJobs.Add(Classes.Warrior, parser.ReadColumn<bool>(22));
		this.ClassJobs.Add(Classes.Dragoon, parser.ReadColumn<bool>(23));
		this.ClassJobs.Add(Classes.Bard, parser.ReadColumn<bool>(24));
		this.ClassJobs.Add(Classes.WhiteMage, parser.ReadColumn<bool>(25));
		this.ClassJobs.Add(Classes.BlackMage, parser.ReadColumn<bool>(26));
		this.ClassJobs.Add(Classes.Arcanist, parser.ReadColumn<bool>(27));
		this.ClassJobs.Add(Classes.Summoner, parser.ReadColumn<bool>(28));
		this.ClassJobs.Add(Classes.Scholar, parser.ReadColumn<bool>(29));
		this.ClassJobs.Add(Classes.Rogue, parser.ReadColumn<bool>(30));
		this.ClassJobs.Add(Classes.Ninja, parser.ReadColumn<bool>(31));
		this.ClassJobs.Add(Classes.Machinist, parser.ReadColumn<bool>(32));
		this.ClassJobs.Add(Classes.DarkKnight, parser.ReadColumn<bool>(33));
		this.ClassJobs.Add(Classes.Astrologian, parser.ReadColumn<bool>(34));
		this.ClassJobs.Add(Classes.Samurai, parser.ReadColumn<bool>(35));
		this.ClassJobs.Add(Classes.RedMage, parser.ReadColumn<bool>(36));
		this.ClassJobs.Add(Classes.BlueMage, parser.ReadColumn<bool>(37));
		this.ClassJobs.Add(Classes.Gunbreaker, parser.ReadColumn<bool>(38));
		this.ClassJobs.Add(Classes.Dancer, parser.ReadColumn<bool>(39));

		// might be backwards:
		this.ClassJobs.Add(Classes.Reaper, parser.ReadColumn<bool>(40));
		this.ClassJobs.Add(Classes.Sage, parser.ReadColumn<bool>(41));
	}

	public bool Contains(Classes classJob)
	{
		if (this.ClassJobs.ContainsKey(classJob))
			return this.ClassJobs[classJob];

		return false;
	}

	public Classes ToFlags()
	{
		Classes classes = Classes.None;

		foreach (Classes? job in Enum.GetValues(typeof(Classes)))
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
