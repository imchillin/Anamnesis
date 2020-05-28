// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using ConceptMatrix.GUI.Serialization;
	using ConceptMatrix.Localization;

	public class LocalizationService : ILocalizationService
	{
		public const string FallbackCulture = "EN";

		private Dictionary<string, Locale> locales = new Dictionary<string, Locale>();

		private Locale fallbackLocale;
		private Locale currentLocale;

		public void Add(string culture, string key, string value)
		{
			culture = culture.ToUpperInvariant();

			if (!this.locales.ContainsKey(culture))
				this.locales.Add(culture, new Locale(culture));

			Locale locale = this.locales[culture];

			this.locales[culture].Add(key, value);
		}

		public void Add(string culture, Dictionary<string, string> values)
		{
			foreach ((string key, string value) in values)
			{
				this.Add(culture, key, value);
			}
		}

		public void Add(string culture, string path)
		{
			string json = File.ReadAllText(path);
			Dictionary<string, string> values = Serializer.Deserialize<Dictionary<string, string>>(json);
			this.Add(culture, values);
		}

		public string GetString(string key)
		{
			string val = this.currentLocale.Get(key);

			if (val == null)
			{
				Log.Write("Missing Localized string: \"" + key + "\" in locale: \"" + this.currentLocale.Culture + "\"");
				val = this.fallbackLocale.Get(key);
			}

			if (val == null)
				throw new Exception("Missing Localized string: \"" + key + "\"");

			return val;
		}

		public Task Initialize()
		{
			this.Add("EN", "Languages/en.json");

			this.currentLocale = this.locales[FallbackCulture];
			this.fallbackLocale = this.currentLocale;
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		private class Locale
		{
			public readonly string Culture;

			private Dictionary<string, string> values = new Dictionary<string, string>();

			public Locale(string culture)
			{
				this.Culture = culture;
			}

			public void Add(string key, string value)
			{
				if (!this.values.ContainsKey(key))
					this.values.Add(key, value);

				this.values[key] = value;
			}

			public string Get(string key)
			{
				if (!this.values.ContainsKey(key))
					return null;

				return this.values[key];
			}
		}
	}
}
