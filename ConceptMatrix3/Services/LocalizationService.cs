// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using ConceptMatrix.GUI.Serialization;
	using ConceptMatrix.Localization;

	public class LocalizationService : ILocalizationService
	{
		public const string FallbackCulture = "EN";

		private readonly Dictionary<string, Locale> locales = new Dictionary<string, Locale>();

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

		public void Add(string searchPath)
		{
			string[] paths = Directory.GetFiles(searchPath);

			foreach (string path in paths)
			{
				string culture = Path.GetFileNameWithoutExtension(path);

				string json = File.ReadAllText(path);
				Dictionary<string, string> values = Serializer.Deserialize<Dictionary<string, string>>(json);
				this.Add(culture, values);
			}
		}

		public string GetString(string key)
		{
			string val;

			if (this.currentLocale.Get(key, out val))
				return val;

			Log.Write("Missing Localized string: \"" + key + "\" in locale: \"" + this.currentLocale.Culture + "\"", "Localization", Log.Severity.Warning);

			if (this.fallbackLocale.Get(key, out val))
				return val;

			Log.Write("Missing Localized string: \"" + key + "\"", "Localization", Log.Severity.Error);
			this.fallbackLocale.Add(key, null);

			return null;
		}

		public Task Initialize()
		{
			this.Add("Languages");

			this.currentLocale = this.locales[FallbackCulture];
			this.fallbackLocale = this.currentLocale;

			this.locales.Add("GIB", new GiberishLocale(this.fallbackLocale));

			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public async Task Start()
		{
			ISettingsService settingsService = ConceptMatrix.Services.Get<ISettingsService>();
			MainApplicationSettings mainSettings = await settingsService.Load<MainApplicationSettings>();

			if (this.locales.ContainsKey(mainSettings.Language))
			{
				this.currentLocale = this.locales[mainSettings.Language];
			}
			else
			{
				this.currentLocale = this.fallbackLocale;
			}
		}

		private class Locale
		{
			public readonly string Culture;

			private Dictionary<string, string> values = new Dictionary<string, string>();

			public Locale(string culture)
			{
				this.Culture = culture;
			}

			public virtual void Add(string key, string value)
			{
				if (!this.values.ContainsKey(key))
					this.values.Add(key, value);

				this.values[key] = value;
			}

			public virtual bool Get(string key, out string value)
			{
				value = null;

				if (!this.values.ContainsKey(key))
					return false;

				value = this.values[key];
				return true;
			}
		}

		/// <summary>
		/// Converts any locale into gibberish English.
		/// </summary>
		private class GiberishLocale : Locale
		{
			private const string Characters = @"abcdefghijklmnopqrstuvwxyz";
			private static Random random = new Random();

			private Locale baseLocale;

			public GiberishLocale(Locale baseLocale)
				: base("Gib")
			{
				this.baseLocale = baseLocale;
			}

			public override bool Get(string key, out string value)
			{
				if (!base.Get(key, out value))
				{
					string str = null;

					if (!this.baseLocale.Get(key, out str))
						return false;

					char[] newStr = new char[str.Length];

					for (int i = 0; i < str.Length; i++)
					{
						if (char.IsDigit(str[i]))
						{
							newStr[i] = random.Next(0, 9).ToString()[0];
						}
						else if (char.IsLetter(str[i]))
						{
							newStr[i] = Characters[random.Next(0, Characters.Length)];

							if (char.IsUpper(str[i]))
							{
								newStr[i] = char.ToUpper(newStr[i]);
							}
						}
						else
						{
							newStr[i] = str[i];
						}
					}

					value = new string(newStr);
					this.Add(key, value);
				}

				return true;
			}
		}
	}
}
