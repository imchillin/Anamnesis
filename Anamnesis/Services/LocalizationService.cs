// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.Serialization;
	using Serilog;
	using XivToolsWpf.DependencyInjection;

	public class LocalizationService : ServiceBase<LocalizationService>, ILocaleProvider
	{
		public const string FallbackCulture = "EN";

		private static readonly Dictionary<string, Locale> Locales = new Dictionary<string, Locale>();

		private static Locale? fallbackLocale;
		private static Locale? currentLocale;

		public event LocalizationEvent? LocaleChanged;

		public static bool Loaded => Exists && currentLocale != null;

		bool ILocaleProvider.Loaded => Loaded;

		public static void Add(string culture, string key, string value)
		{
			culture = culture.ToUpperInvariant();

			if (!Locales.ContainsKey(culture))
				Locales.Add(culture, new Locale(culture));

			Locale locale = Locales[culture];

			Locales[culture].Add(key, value);
		}

		public static void Add(string culture, Dictionary<string, string> values)
		{
			foreach ((string key, string value) in values)
			{
				Add(culture, key, value);
			}
		}

		public static void Add(string searchPath)
		{
			string[] paths = Directory.GetFiles(searchPath);

			foreach (string path in paths)
			{
				string culture = Path.GetFileNameWithoutExtension(path);

				string json = File.ReadAllText(path);

				try
				{
					Dictionary<string, string> values = SerializerService.Deserialize<Dictionary<string, string>>(json);
					Add(culture, values);
				}
				catch (Exception ex)
				{
					Log.Error(ex, $"Failed to load language: {path}");
				}
			}
		}

		public static bool HasString(string key)
		{
			string val;
			if (currentLocale?.Get(key, out val) != true)
				return false;

			return true;
		}

		public static string GetStringFormatted(string key, params string[] param)
		{
			string str = GetString(key);
			return string.Format(str, param);
		}

		public static string GetStringAllLanguages(string key)
		{
			StringBuilder builder = new StringBuilder();

			foreach ((string code, Locale locale) in Locales)
			{
				string val;
				if (locale.Get(key, out val))
				{
					builder.AppendLine(val);
					builder.AppendLine();
				}
			}

			return builder.ToString().TrimEnd();
		}

		public static string GetString(string key, bool silent = false)
		{
			string val = string.Empty;

			if (currentLocale?.Get(key, out val) ?? false)
				return val;

			if (!silent)
				Log.Warning("Missing Localized string: \"" + key + "\" in locale: \"" + currentLocale?.Culture + "\"");

			if (fallbackLocale?.Get(key, out val) ?? false)
				return val;

			if (!silent)
				Log.Error("Missing Localized string: \"" + key + "\"");

			if (!silent)
			{
				string erorString = "{" + key + "}";
				fallbackLocale?.Add(key, erorString);
				return erorString;
			}
			else
			{
				fallbackLocale?.Add(key, string.Empty);
				return string.Empty;
			}
		}

		public static void SetLocale(string locale)
		{
			locale = locale.ToUpper();

			if (Locales.ContainsKey(locale))
			{
				currentLocale = Locales[locale];
			}
			else
			{
				currentLocale = fallbackLocale;
			}

			Instance.LocaleChanged?.Invoke();
		}

		public static Dictionary<string, string> GetAvailableLocales()
		{
			Dictionary<string, string> results = new Dictionary<string, string>();
			foreach (Locale locale in Locales.Values)
			{
				if (results.ContainsKey(locale.Culture))
					continue;

				results.Add(locale.Culture, locale.Name);
			}

			return results;
		}

		public override async Task Initialize()
		{
			DependencyFactory.RegisterDependency<ILocaleProvider>(this);

			await base.Initialize();

			Add("Languages");

			currentLocale = Locales[FallbackCulture];
			fallbackLocale = currentLocale;
		}

		public override async Task Start()
		{
			await base.Start();

			SetLocale(SettingsService.Current.Language);
		}

		bool ILocaleProvider.HasString(string key) => HasString(key);
		string ILocaleProvider.GetStringFormatted(string key, params string[] param) => GetStringFormatted(key, param);
		string ILocaleProvider.GetStringAllLanguages(string key) => GetStringAllLanguages(key);
		string ILocaleProvider.GetString(string key, bool silent) => GetString(key, silent);

		private class Locale
		{
			public readonly string Culture;
			public string Name;

			private readonly Dictionary<string, string> values = new Dictionary<string, string>();

			public Locale(string culture)
			{
				this.Culture = culture;
				this.Name = culture;
			}

			public virtual void Add(string key, string value)
			{
				if (!this.values.ContainsKey(key))
					this.values.Add(key, value);

				this.values[key] = value;
			}

			public virtual bool Get(string key, out string value)
			{
				value = string.Empty;

				if (!this.values.ContainsKey(key))
					return false;

				value = this.values[key];
				return true;
			}
		}
	}
}
