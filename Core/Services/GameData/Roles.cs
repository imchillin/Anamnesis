// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;

	#pragma warning disable SA1649

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
		private static Dictionary<Roles, List<Classes>>? classLookup;

		public static List<Classes> GetClasses(this Roles role)
		{
			if (classLookup == null)
			{
				classLookup = new Dictionary<Roles, List<Classes>>();

				foreach (Classes job in Enum.GetValues(typeof(Classes)))
				{
					if (job == Classes.None)
						continue;

					Roles? classRole = job.GetRole();

					if (classRole == null)
						continue;

					if (!classLookup.ContainsKey((Roles)classRole))
						classLookup.Add((Roles)classRole, new List<Classes>());

					classLookup[(Roles)classRole].Add(job);
				}
			}

			return classLookup[role];
		}

		public static string GetName(this Roles role)
		{
			return role.ToString();
		}
	}
}
