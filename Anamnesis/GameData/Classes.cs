// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.Core.Extensions;
using Anamnesis.GameData.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	Viper = 1L << 41,
	Pictomancer = 1L << 42,

	All = Alchemist | Arcanist | Archer | Armorer | Astrologian | Bard | BlackMage | Blacksmith | BlueMage | Botanist
		| Carpenter | Conjurer | Culinarian | Dancer | DarkKnight | Dragoon | Fisher | Gladiator | Goldsmith | Gunbreaker
		| Lancer | Leatherworker | Machinist | Marauder | Miner | Monk | Ninja | Paladin | Pugilist | RedMage | Rogue
		| Samurai | Scholar | Summoner | Thaumaturge | Warrior | Weaver | WhiteMage | Reaper | Sage | Viper | Pictomancer,
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
			case Classes.Viper: return "Viper";
			case Classes.Pictomancer: return "Pictomancer";
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
			case Classes.Viper: return Roles.Damage;
			case Classes.Pictomancer: return Roles.Damage;
		}

		throw new Exception($"No role for class/job: {job}");
	}

	public static ImgRef GetIcon(this Classes job) => new(job.GetIconId());

	public static int GetIconId(this Classes job)
	{
		return job switch
		{
			Classes.Alchemist => 062014,
			Classes.Arcanist => 062026,
			Classes.Archer => 062005,
			Classes.Armorer => 062010,
			Classes.Astrologian => 062033,
			Classes.Bard => 062023,
			Classes.BlackMage => 062025,
			Classes.Blacksmith => 062009,
			Classes.BlueMage => 062036,
			Classes.Botanist => 062017,
			Classes.Carpenter => 062008,
			Classes.Conjurer => 062006,
			Classes.Culinarian => 062015,
			Classes.Dancer => 062038,
			Classes.DarkKnight => 062032,
			Classes.Dragoon => 062022,
			Classes.Fisher => 062018,
			Classes.Gladiator => 062001,
			Classes.Goldsmith => 062011,
			Classes.Gunbreaker => 062037,
			Classes.Lancer => 062004,
			Classes.Leatherworker => 062012,
			Classes.Machinist => 062031,
			Classes.Marauder => 062003,
			Classes.Miner => 062016,
			Classes.Monk => 062020,
			Classes.Ninja => 062030,
			Classes.Paladin => 062019,
			Classes.Pictomancer => 062042,
			Classes.Pugilist => 062002,
			Classes.Reaper => 062039,
			Classes.RedMage => 062035,
			Classes.Rogue => 062029,
			Classes.Sage => 062040,
			Classes.Samurai => 062034,
			Classes.Scholar => 062028,
			Classes.Summoner => 062027,
			Classes.Thaumaturge => 062007,
			Classes.Viper => 062041,
			Classes.Warrior => 062021,
			Classes.Weaver => 062013,
			Classes.WhiteMage => 062024,
			_ => throw new Exception($"No icon for class/job: {job}"),
		};
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

			if (!self.HasFlagUnsafe(job))
			{
				return false;
			}
		}

		return true;
	}

	public static bool IsClass(this Classes self)
	{
		return self switch
		{
			Classes.Gladiator => true,
			Classes.Marauder => true,
			Classes.Conjurer => true,
			Classes.Arcanist => true,
			Classes.Archer => true,
			Classes.Lancer => true,
			Classes.Pugilist => true,
			Classes.Rogue => true,
			Classes.Thaumaturge => true,
			_ => false,
		};
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

		List<Classes> selected = new();
		foreach (Classes? job in Enum.GetValues<Classes>().Select(v => (Classes?)v))
		{
			if (job == null)
				continue;

			Classes cls = (Classes)job;

			if (!cls.IsClass() && !cls.IsJob())
				continue;

			if (onlyJobs && !cls.IsJob())
				continue;

			if (self.HasFlagUnsafe(cls))
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
		HashSet<Roles> entireRoles = new();
		foreach (Roles? role in Enum.GetValues<Roles>().Select(v => (Roles?)v))
		{
			if (role == null)
				continue;

			if (self.HasAllClassesInRole((Roles)role, onlyJobs))
			{
				entireRoles.Add((Roles)role);
			}
		}

		// check if there are any extra classes that werent part of the enire role
		foreach (Roles? role in Enum.GetValues<Roles>().Select(v => (Roles?)v))
		{
			if (role == null)
				continue;

			if (entireRoles.Contains((Roles)role))
				continue;

			foreach (Classes job in ((Roles)role).GetClasses())
			{
				if (onlyJobs && !job.IsJob())
					continue;

				if (self.HasFlagUnsafe(job))
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
