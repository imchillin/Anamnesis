// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule
{
	using System.Collections.Generic;
	using Anamnesis.Memory;

	public static class ActorExtensions
	{
		private static Dictionary<int, bool> isCustomizableLookup;

		public static bool IsCustomizable(this ActorViewModel self)
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

			if (!isCustomizableLookup.ContainsKey(self.ModelType))
				return false;

			return isCustomizableLookup[self.ModelType];
		}
	}
}
