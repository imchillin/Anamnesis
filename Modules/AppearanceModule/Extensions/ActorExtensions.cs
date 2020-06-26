// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule
{
	using System.Collections.Generic;

	public static class ActorExtensions
	{
		private static Dictionary<int, bool> isCustomizableLookup;

		public static bool IsCustomizable(this Actor self)
		{
			if (isCustomizableLookup == null)
			{
				isCustomizableLookup = new Dictionary<int, bool>();

				foreach (ModelTypes modelType in Module.ModelTypes)
				{
					if (!isCustomizableLookup.ContainsKey(modelType.Id))
						isCustomizableLookup.Add(modelType.Id, modelType.CanCustomize);

					isCustomizableLookup[modelType.Id] |= modelType.CanCustomize;
				}
			}

			int modelTypeId = self.GetValue(Offsets.Main.ModelType);
			if (!isCustomizableLookup.ContainsKey(modelTypeId))
				return false;

			return isCustomizableLookup[modelTypeId];
		}
	}
}
