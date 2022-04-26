// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using System;
using System.Collections.Generic;
using System.Text;
using Anamnesis.GameData.Sheets;

#pragma warning disable SA1649

[Flags]
public enum Classes : long
{
	None = 0,

	Alchemist = 1L << 1,
	Arcanist = 1L << 2,
	Archer = 1L << 3,
	Armorer = 1L << 4,
	Astrologian = 1L << 5,
	Bard = 1L << 6,
	BlackMage = 1L << 7,
	Blacksmith = 1L << 8,
	BlueMage = 1L << 9,
	Botanist = 1L << 10,
	Carpenter = 1L << 11,
	Conjurer = 1L << 12,
	Culinarian = 1L << 13,
	Dancer = 1L << 14,
	DarkKnight = 1L << 15,
	Dragoon = 1L << 16,
	Fisher = 1L << 17,
	Gladiator = 1L << 18,
	Goldsmith = 1L << 19,
	Gunbreaker = 1L << 20,
	Lancer = 1L << 21,
	Leatherworker = 1L << 22,
	Machinist = 1L << 23,
	Marauder = 1L << 24,
	Miner = 1L << 25,
	Monk = 1L << 26,
	Ninja = 1L << 27,
	Paladin = 1L << 28,
	Pugilist = 1L << 29,
	RedMage = 1L << 30,
	Rogue = 1L << 31,
	Samurai = 1L << 32,
	Scholar = 1L << 33,
	Summoner = 1L << 34,
	Thaumaturge = 1L << 35,
	Warrior = 1L << 36,
	Weaver = 1L << 37,
	WhiteMage = 1L << 38,
	Reaper = 1L << 39,
	Sage = 1L << 40,

	All = Alchemist | Arcanist | Archer | Armorer | Astrologian | Bard | BlackMage | Blacksmith | BlueMage | Botanist
		| Carpenter | Conjurer | Culinarian | Dancer | DarkKnight | Dragoon | Fisher | Gladiator | Goldsmith | Gunbreaker
		| Lancer | Leatherworker | Machinist | Marauder | Miner | Monk | Ninja | Paladin | Pugilist | RedMage | Rogue
		| Samurai | Scholar | Summoner | Thaumaturge | Warrior | Weaver | WhiteMage | Reaper | Sage,
}

public static class ClassesExtensions
{
	public static string? GetName(this Classes job)
	{
		switch (job)
		{
			case Classes.Alchemist: return "Alchemist";
			case Classes.Arcanist: return "Arcanist";
			case Classes.Archer: return "Archer";
			case Classes.Armorer: return "Armorer";
			case Classes.Astrologian: return "Astrologian";
			case Classes.Bard: return "Bard";
			case Classes.BlackMage: return "Black Mage";
			case Classes.Blacksmith: return "Blacksmith";
			case Classes.BlueMage: return "Blue Mage";
			case Classes.Botanist: return "Botanist";
			case Classes.Carpenter: return "Carpenter";
			case Classes.Conjurer: return "Conjurer";
			case Classes.Culinarian: return "Culinarian";
			case Classes.Dancer: return "Dancer";
			case Classes.DarkKnight: return "Dark Knight";
			case Classes.Dragoon: return "Dragoon";
			case Classes.Fisher: return "Fisher";
			case Classes.Gladiator: return "Gladiator";
			case Classes.Goldsmith: return "Goldsmith";
			case Classes.Gunbreaker: return "Gunbreaker";
			case Classes.Lancer: return "Lancer";
			case Classes.Leatherworker: return "Leatherworker";
			case Classes.Machinist: return "Machinist";
			case Classes.Marauder: return "Marauder";
			case Classes.Miner: return "Miner";
			case Classes.Monk: return "Monk";
			case Classes.Ninja: return "Ninja";
			case Classes.Paladin: return "Paladin";
			case Classes.Pugilist: return "Pugilist";
			case Classes.RedMage: return "Red Mage";
			case Classes.Rogue: return "Rogue";
			case Classes.Samurai: return "Samurai";
			case Classes.Scholar: return "Scholar";
			case Classes.Summoner: return "Summoner";
			case Classes.Thaumaturge: return "Thaumaturge";
			case Classes.Warrior: return "Warrior";
			case Classes.Weaver: return "Weaver";
			case Classes.WhiteMage: return "White Mage";
			case Classes.Reaper: return "Reaper";
			case Classes.Sage: return "Sage";
		}

		throw new Exception($"No name for class/job: {job}");
	}

	public static Roles? GetRole(this Classes job)
	{
		switch (job)
		{
			case Classes.None: return null;
			case Classes.All: return null;

			case Classes.Alchemist: return Roles.Crafters;
			case Classes.Arcanist: return Roles.Damage;
			case Classes.Archer: return Roles.Damage;
			case Classes.Armorer: return Roles.Crafters;
			case Classes.Astrologian: return Roles.Healers;
			case Classes.Bard: return Roles.Damage;
			case Classes.BlackMage: return Roles.Damage;
			case Classes.Blacksmith: return Roles.Crafters;
			case Classes.BlueMage: return Roles.Damage;
			case Classes.Botanist: return Roles.Gatherers;
			case Classes.Carpenter: return Roles.Crafters;
			case Classes.Conjurer: return Roles.Healers;
			case Classes.Culinarian: return Roles.Crafters;
			case Classes.Dancer: return Roles.Damage;
			case Classes.DarkKnight: return Roles.Tanks;
			case Classes.Dragoon: return Roles.Damage;
			case Classes.Fisher: return Roles.Gatherers;
			case Classes.Gladiator: return Roles.Tanks;
			case Classes.Goldsmith: return Roles.Crafters;
			case Classes.Gunbreaker: return Roles.Tanks;
			case Classes.Lancer: return Roles.Damage;
			case Classes.Leatherworker: return Roles.Crafters;
			case Classes.Machinist: return Roles.Damage;
			case Classes.Marauder: return Roles.Tanks;
			case Classes.Miner: return Roles.Gatherers;
			case Classes.Monk: return Roles.Damage;
			case Classes.Ninja: return Roles.Damage;
			case Classes.Paladin: return Roles.Tanks;
			case Classes.Pugilist: return Roles.Damage;
			case Classes.RedMage: return Roles.Damage;
			case Classes.Rogue: return Roles.Damage;
			case Classes.Samurai: return Roles.Damage;
			case Classes.Scholar: return Roles.Healers;
			case Classes.Summoner: return Roles.Damage;
			case Classes.Thaumaturge: return Roles.Damage;
			case Classes.Warrior: return Roles.Tanks;
			case Classes.Weaver: return Roles.Crafters;
			case Classes.WhiteMage: return Roles.Healers;
			case Classes.Reaper: return Roles.Damage;
			case Classes.Sage: return Roles.Healers;
		}

		throw new Exception($"No role for class/job: {job}");
	}

	public static ImageReference GetIcon(this Classes job)
	{
		return new ImageReference(job.GetIconId());
	}

	public static int GetIconId(this Classes job)
	{
		switch (job)
		{
			case Classes.Alchemist: return 062014;
			case Classes.Arcanist: return 062026;
			case Classes.Archer: return 062005;
			case Classes.Armorer: return 062010;
			case Classes.Astrologian: return 062033;
			case Classes.Bard: return 062023;
			case Classes.BlackMage: return 062025;
			case Classes.Blacksmith: return 062009;
			case Classes.BlueMage: return 062036;
			case Classes.Botanist: return 062017;
			case Classes.Carpenter: return 062008;
			case Classes.Conjurer: return 062006;
			case Classes.Culinarian: return 062015;
			case Classes.Dancer: return 062038;
			case Classes.DarkKnight: return 062032;
			case Classes.Dragoon: return 062022;
			case Classes.Fisher: return 062018;
			case Classes.Gladiator: return 062001;
			case Classes.Goldsmith: return 062011;
			case Classes.Gunbreaker: return 062037;
			case Classes.Lancer: return 062004;
			case Classes.Leatherworker: return 062012;
			case Classes.Machinist: return 062031;
			case Classes.Marauder: return 062003;
			case Classes.Miner: return 062016;
			case Classes.Monk: return 062020;
			case Classes.Ninja: return 062030;
			case Classes.Paladin: return 062019;
			case Classes.Pugilist: return 062002;
			case Classes.Reaper: return 062039;
			case Classes.RedMage: return 062035;
			case Classes.Rogue: return 062029;
			case Classes.Sage: return 062040;
			case Classes.Samurai: return 062034;
			case Classes.Scholar: return 062028;
			case Classes.Summoner: return 062027;
			case Classes.Thaumaturge: return 062007;
			case Classes.Warrior: return 062021;
			case Classes.Weaver: return 062013;
			case Classes.WhiteMage: return 062024;
		}

		throw new Exception($"No icon for class/job: {job}");
	}

	public static Classes SetFlag(this Classes a, Classes b, bool enabled)
	{
		if (enabled)
		{
			return a | b;
		}
		else
		{
			return a & ~b;
		}
	}

	public static bool HasAllClassesInRole(this Classes self, Roles role, bool onlyJobs = false)
	{
		foreach (Classes job in role.GetClasses())
		{
			if (onlyJobs && !job.IsJob())
				continue;

			if (!self.HasFlag(job))
			{
				return false;
			}
		}

		return true;
	}

	public static bool IsClass(this Classes self)
	{
		switch (self)
		{
			case Classes.Gladiator: return true;
			case Classes.Marauder: return true;
			case Classes.Conjurer: return true;
			case Classes.Arcanist: return true;
			case Classes.Archer: return true;
			case Classes.Lancer: return true;
			case Classes.Pugilist: return true;
			case Classes.Rogue: return true;
			case Classes.Thaumaturge: return true;
		}

		return false;
	}

	public static bool IsJob(this Classes self)
	{
		if (self == Classes.None || self == Classes.All)
			return false;

		return !self.IsClass();
	}

	public static string Describe(this Classes self, bool onlyJobs = false)
	{
		if (self == Classes.All)
			return "All";

		if (self == Classes.None)
			return "None";

		List<Classes> selected = new List<Classes>();
		foreach (Classes? job in Enum.GetValues(typeof(Classes)))
		{
			if (job == null)
				continue;

			Classes cls = (Classes)job;

			if (!cls.IsClass() && !cls.IsJob())
				continue;

			if (onlyJobs && !cls.IsJob())
				continue;

			if (self.HasFlag(cls))
			{
				selected.Add(cls);
			}
		}

		if (selected.Count == 0)
		{
			return "None";
		}
		else if (selected.Count == 1)
		{
			string? name = selected[0].GetName();

			if (name == null)
				throw new Exception("Failed to get name of class: " + selected[0]);

			return (string)name;
		}

		// Get all the roles that are entirely included
		HashSet<Roles> entireRoles = new HashSet<Roles>();
		foreach (Roles? role in Enum.GetValues(typeof(Roles)))
		{
			if (role == null)
				continue;

			if (self.HasAllClassesInRole((Roles)role, onlyJobs))
			{
				entireRoles.Add((Roles)role);
			}
		}

		// check if there are any extra classes that werent part of the enire role
		foreach (Roles? role in Enum.GetValues(typeof(Roles)))
		{
			if (role == null)
				continue;

			if (entireRoles.Contains((Roles)role))
				continue;

			foreach (Classes job in ((Roles)role).GetClasses())
			{
				if (onlyJobs && !job.IsJob())
					continue;

				if (self.HasFlag(job))
				{
					return "Mixed";
				}
			}
		}

		int index = 0;
		StringBuilder builder = new StringBuilder();
		foreach (Roles role in entireRoles)
		{
			if (index > 0)
				builder.Append(", ");

			builder.Append(role.GetName());
			index++;
		}

		return builder.ToString();
	}
}
