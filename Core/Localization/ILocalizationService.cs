// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Localization
{
	using System.Collections.Generic;

	public interface ILocalizationService : IService
	{
		string GetString(string key);

		void Add(string culture, string key, string value);
		void Add(string culture, Dictionary<string, string> values);
		void Add(string searchPath);
	}
}
