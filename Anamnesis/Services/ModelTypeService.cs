// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using Anamnesis.Character;
	using Anamnesis.Serialization;
	using SimpleLog;

	public class ModelTypeService : ServiceBase<ModelTypeService>
	{
		private static Dictionary<int, bool> isCustomizableLookup = new Dictionary<int, bool>();

		public static ReadOnlyCollection<ModelTypes> ModelTypes { get; private set; } = new List<ModelTypes>().AsReadOnly();

		public static bool IsCustomizable(int modelTypeId)
		{
			if (modelTypeId == 0)
				return true;

			if (!isCustomizableLookup.ContainsKey(modelTypeId))
				return false;

			return isCustomizableLookup[modelTypeId];
		}

		public override async Task Start()
		{
			await base.Start();

			try
			{
				List<ModelTypes> modelTypes = SerializerService.DeserializeFile<List<ModelTypes>>("ModelTypes.json");
				ModelTypes = modelTypes.AsReadOnly();

				foreach (ModelTypes modelType in ModelTypes)
				{
					if (!isCustomizableLookup.ContainsKey(modelType.Id))
						isCustomizableLookup.Add(modelType.Id, modelType.CanCustomize);

					isCustomizableLookup[modelType.Id] |= modelType.CanCustomize;
				}
			}
			catch (Exception ex)
			{
				Log.Write(Severity.Error, new Exception("Failed to load model type list", ex));
			}
		}
	}
}
