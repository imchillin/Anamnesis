// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.Core.Extensions;
using Anamnesis.GameData.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

/// <summary>Represents a class or job in the game.</summary>
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
	/// <summary>
	/// Gets the display name of a class/job.
	/// </summary>
	/// <param name="job">The class/job to get the name of.</param>
	/// <returns>The name of the class/job.</returns>
	/// <exception cref="Exception">Thrown if the class/job does not have a name.</exception>
	public static string? GetName(this Classes job)
	{
		return job switch
		{
			Classes.Alchemist => "Alchemist",
			Classes.Arcanist => "Arcanist",
			Classes.Archer => "Archer",
			Classes.Armorer => "Armorer",
			Classes.Astrologian => "Astrologian",
			Classes.Bard => "Bard",
			Classes.BlackMage => "Black Mage",
			Classes.Blacksmith => "Blacksmith",
			Classes.BlueMage => "Blue Mage",
			Classes.Botanist => "Botanist",
			Classes.Carpenter => "Carpenter",
			Classes.Conjurer => "Conjurer",
			Classes.Culinarian => "Culinarian",
			Classes.Dancer => "Dancer",
			Classes.DarkKnight => "Dark Knight",
			Classes.Dragoon => "Dragoon",
			Classes.Fisher => "Fisher",
			Classes.Gladiator => "Gladiator",
			Classes.Goldsmith => "Goldsmith",
			Classes.Gunbreaker => "Gunbreaker",
			Classes.Lancer => "Lancer",
			Classes.Leatherworker => "Leatherworker",
			Classes.Machinist => "Machinist",
			Classes.Marauder => "Marauder",
			Classes.Miner => "Miner",
			Classes.Monk => "Monk",
			Classes.Ninja => "Ninja",
			Classes.Paladin => "Paladin",
			Classes.Pugilist => "Pugilist",
			Classes.RedMage => "Red Mage",
			Classes.Rogue => "Rogue",
			Classes.Samurai => "Samurai",
			Classes.Scholar => "Scholar",
			Classes.Summoner => "Summoner",
			Classes.Thaumaturge => "Thaumaturge",
			Classes.Warrior => "Warrior",
			Classes.Weaver => "Weaver",
			Classes.WhiteMage => "White Mage",
			Classes.Reaper => "Reaper",
			Classes.Sage => "Sage",
			Classes.Viper => "Viper",
			Classes.Pictomancer => "Pictomancer",
			_ => throw new Exception($"No name for class/job: {job}"),
		};
	}

	/// <summary>
	/// Gets the role of the given class/job.
	/// </summary>
	/// <param name="job">The class/job to get the role of.</param>
	/// <returns>The role of the class/job.</returns>
	/// <exception cref="Exception">Thrown if the class/job does not have a role.</exception>
	public static Roles? GetRole(this Classes job)
	{
		return job switch
		{
			Classes.None => null,
			Classes.All => null,
			Classes.Alchemist => Roles.Crafters,
			Classes.Arcanist => Roles.Damage,
			Classes.Archer => Roles.Damage,
			Classes.Armorer => Roles.Crafters,
			Classes.Astrologian => Roles.Healers,
			Classes.Bard => Roles.Damage,
			Classes.BlackMage => Roles.Damage,
			Classes.Blacksmith => Roles.Crafters,
			Classes.BlueMage => Roles.Damage,
			Classes.Botanist => Roles.Gatherers,
			Classes.Carpenter => Roles.Crafters,
			Classes.Conjurer => Roles.Healers,
			Classes.Culinarian => Roles.Crafters,
			Classes.Dancer => Roles.Damage,
			Classes.DarkKnight => Roles.Tanks,
			Classes.Dragoon => Roles.Damage,
			Classes.Fisher => Roles.Gatherers,
			Classes.Gladiator => Roles.Tanks,
			Classes.Goldsmith => Roles.Crafters,
			Classes.Gunbreaker => Roles.Tanks,
			Classes.Lancer => Roles.Damage,
			Classes.Leatherworker => Roles.Crafters,
			Classes.Machinist => Roles.Damage,
			Classes.Marauder => Roles.Tanks,
			Classes.Miner => Roles.Gatherers,
			Classes.Monk => Roles.Damage,
			Classes.Ninja => Roles.Damage,
			Classes.Paladin => Roles.Tanks,
			Classes.Pictomancer => Roles.Damage,
			Classes.Pugilist => Roles.Damage,
			Classes.Reaper => Roles.Damage,
			Classes.RedMage => Roles.Damage,
			Classes.Rogue => Roles.Damage,
			Classes.Sage => Roles.Healers,
			Classes.Samurai => Roles.Damage,
			Classes.Scholar => Roles.Healers,
			Classes.Summoner => Roles.Damage,
			Classes.Thaumaturge => Roles.Damage,
			Classes.Viper => Roles.Damage,
			Classes.Warrior => Roles.Tanks,
			Classes.Weaver => Roles.Crafters,
			Classes.WhiteMage => Roles.Healers,
			_ => throw new Exception($"No role for class/job: {job}"),
		};
	}

	/// <summary>
	/// Gets the icon for a class/job.
	/// </summary>
	/// <param name="job">The class/job to get the icon for.</param>
	/// <returns>An image reference to the icon.</returns>
	public static ImgRef GetIcon(this Classes job) => new(job.GetIconId());

	/// <summary>
	/// Gets the icon ID for a class/job.
	/// </summary>
	/// <param name="job">The class/job to get the icon ID for.</param>
	/// <returns>The icon ID.</returns>
	/// <exception cref="Exception">Thrown if the class/job does not have an icon ID.</exception>
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Classes SetFlag(this Classes a, Classes b, bool enabled) => enabled ? a | b : a & ~b;

	/// <summary>
	/// Determines if this class/job is included in the given classes.
	/// </summary>
	/// <param name="self">The class/job to check.</param>
	/// <param name="role">The classes to check against.</param>
	/// <param name="onlyJobs">True if only jobs should be checked, false if classes should be checked.</param>
	/// <returns>The result of the check.</returns>
	public static bool HasAllClassesInRole(this Classes self, Roles role, bool onlyJobs = false)
	{
		foreach (Classes job in role.GetClasses())
		{
			if (onlyJobs && !job.IsJob())
				continue;

			if (!self.HasFlagUnsafe(job))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Determines if this is a class, and not a job.
	/// </summary>
	/// <param name="self">The class to check.</param>
	/// <returns>True if this is a class, false if it is a job.</returns>
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

	/// <summary>
	/// Determines if this is a job, and not a class.
	/// </summary>
	/// <param name="self">The class to check.</param>
	/// <returns>True if this is a job, false if it is a class.</returns>
	public static bool IsJob(this Classes self)
	{
		if (self == Classes.None || self == Classes.All)
			return false;

		return !self.IsClass();
	}

	/// <summary>
	/// Creates a description of the given classes.
	/// </summary>
	/// <param name="self">The classes to describe.</param>
	/// <param name="onlyJobs">True if only jobs should be included in the description, false if classes should be included.</param>
	/// <returns>A description of the given classes.</returns>
	/// <exception cref="Exception">Thrown if a class/job does not have a name.</exception>
	public static string Describe(this Classes self, bool onlyJobs = false)
	{
		if (self == Classes.All)
			return "All";

		if (self == Classes.None)
			return "None";

		List<Classes> selected = [];
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
			return selected[0].GetName() ?? throw new Exception("Failed to get name of class: " + selected[0]);
		}

		// Get all the roles that are entirely included
		HashSet<Roles> entireRoles = [];
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
		StringBuilder builder = new();
		foreach (Roles role in entireRoles)
		{
			if (index > 0)
				builder.Append(", ");

			builder.Append(role.ToString());
			index++;
		}

		return builder.ToString();
	}
}
