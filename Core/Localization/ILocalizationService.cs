// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Localization
{
	using System.Collections.Generic;

	public delegate void LocalizationEvent();

	public interface ILocalizationService : IService
	{
		event LocalizationEvent LocaleChanged;

		void SetLocale(string culture);
		Dictionary<string, string> GetAvailableLocales();

		string GetString(string key);

		void Add(string culture, string key, string value);
		void Add(string culture, Dictionary<string, string> values);
		void Add(string searchPath);
	}
}
