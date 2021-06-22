// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Text;

#pragma warning disable SA1649

	[Flags]
	public enum Classes : long
	{
		None = 0,

		Alchemist = 1,
		Arcanist = 2,
		Archer = 4,
		Armorer = 8,
		Astrologian = 16,
		Bard = 32,
		BlackMage = 64,
		Blacksmith = 128,
		BlueMage = 256,
		Botanist = 512,
		Carpenter = 1024,
		Conjurer = 2048,
		Culinarian = 4096,
		Dancer = 8192,
		DarkKnight = 16384,
		Dragoon = 32768,
		Fisher = 65536,
		Gladiator = 131072,
		Goldsmith = 262144,
		Gunbreaker = 524288,
		Lancer = 1048576,
		Leatherworker = 2097152,
		Machinist = 4194304,
		Marauder = 8388608,
		Miner = 16777216,
		Monk = 33554432,
		Ninja = 67108864,
		Paladin = 134217728,
		Pugilist = 268435456,
		RedMage = 536870912,
		Rogue = 1073741824,
		Samurai = 2147483648,
		Scholar = 4294967296,
		Summoner = 8589934592,
		Thaumaturge = 17179869184,
		Warrior = 34359738368,
		Weaver = 68719476736,
		WhiteMage = 137438953472,

		All = Alchemist | Arcanist | Archer | Armorer | Astrologian | Bard | BlackMage | Blacksmith | BlueMage | Botanist
			| Carpenter | Conjurer | Culinarian | Dancer | DarkKnight | Dragoon | Fisher | Gladiator | Goldsmith | Gunbreaker
			| Lancer | Leatherworker | Machinist | Marauder | Miner | Monk | Ninja | Paladin | Pugilist | RedMage | Rogue
			| Samurai | Scholar | Summoner | Thaumaturge | Warrior | Weaver | WhiteMage,
	}

	public static class ClassesExtensions
	{
		public static string GetAbbreviation(this Classes job)
		{
			switch (job)
			{
				case Classes.Alchemist: return "ALC";
				case Classes.Arcanist: return "ACN";
				case Classes.Archer: return "ARC";
				case Classes.Armorer: return "ARM";
				case Classes.Astrologian: return "AST";
				case Classes.Bard: return "BRD";
				case Classes.BlackMage: return "BLM";
				case Classes.Blacksmith: return "BSM";
				case Classes.BlueMage: return "BLU";
				case Classes.Botanist: return "BTN";
				case Classes.Carpenter: return "CRP";
				case Classes.Conjurer: return "CNJ";
				case Classes.Culinarian: return "CUL";
				case Classes.Dancer: return "DNC";
				case Classes.DarkKnight: return "DRK";
				case Classes.Dragoon: return "DRG";
				case Classes.Fisher: return "FSH";
				case Classes.Gladiator: return "GLA";
				case Classes.Goldsmith: return "GSM";
				case Classes.Gunbreaker: return "GNB";
				case Classes.Lancer: return "LNC";
				case Classes.Leatherworker: return "LTW";
				case Classes.Machinist: return "MCH";
				case Classes.Marauder: return "MRD";
				case Classes.Miner: return "MIN";
				case Classes.Monk: return "MNK";
				case Classes.Ninja: return "NIN";
				case Classes.Paladin: return "PLD";
				case Classes.Pugilist: return "PGL";
				case Classes.RedMage: return "RDM";
				case Classes.Rogue: return "ROG";
				case Classes.Samurai: return "SAM";
				case Classes.Scholar: return "SCH";
				case Classes.Summoner: return "SMN";
				case Classes.Thaumaturge: return "THM";
				case Classes.Warrior: return "WAR";
				case Classes.Weaver: return "WVR";
				case Classes.WhiteMage: return "WHM";
			}

			throw new Exception("No abbreviation for class: " + job);
		}

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
			}

			return null;
		}

		public static Roles? GetRole(this Classes job)
		{
			switch (job)
			{
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
			}

			return null;
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
					throw new Exception("Faield to get name of class: " + selected[0]);

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
}
