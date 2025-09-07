// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Represents a role that a class can have.</summary>
public enum Roles
{
	Tanks,
	Healers,
	Damage,
	Gatherers,
	Crafters,
}

public static class RolesExtensions
{
	private static Dictionary<Roles, List<Classes>>? s_classLookupCache;

	/// <summary>
	/// Gets all classes that are part of this role.
	/// </summary>
	/// <param name="role">The role to get the classes for.</param>
	/// <returns>A collection of classes that are part of this role.</returns>
	public static List<Classes> GetClasses(this Roles role)
	{
		if (s_classLookupCache == null)
		{
			s_classLookupCache = [];

			foreach (Classes? job in Enum.GetValues<Classes>().Select(v => (Classes?)v))
			{
				if (job == null || job == Classes.None)
					continue;

				Roles? classRole = ((Classes)job).GetRole();

				if (classRole == null)
					continue;

				if (!s_classLookupCache.ContainsKey((Roles)classRole))
					s_classLookupCache.Add((Roles)classRole, []);

				s_classLookupCache[(Roles)classRole].Add((Classes)job);
			}
		}

		return s_classLookupCache[role];
	}
}
