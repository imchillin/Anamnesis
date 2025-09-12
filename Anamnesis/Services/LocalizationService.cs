// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Files;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XivToolsWpf.DependencyInjection;

/// <summary>
/// A singleton service that handles the application's localization.
/// </summary>
public class LocalizationService : ServiceBase<LocalizationService>, ILocaleProvider
{
	public const string FALLBACK_CULTURE = "EN";

	private static readonly Dictionary<string, Locale> s_locales = new(StringComparer.OrdinalIgnoreCase);

	private static Locale? s_fallbackLocale;
	private static Locale? s_currentLocale;

	/// <inheritdoc/>
	public event LocalizationEvent? LocaleChanged;

	/// <summary>
	/// Gets a value indicating whether the locale provider is loaded.
	/// </summary>
	public static bool Loaded => Instance.IsInitialized && s_currentLocale != null;

	/// <inheritdoc/>
	bool ILocaleProvider.Loaded => Loaded;

	/// <summary>
	/// Adds a single localized string to the specified locale.
	/// </summary>
	/// <param name="culture">The two-letter culture code of the target locale.</param>
	/// <param name="cultureName">The name of the target locale (created if missing).</param>
	/// <param name="key">The key of the translation.</param>
	/// <param name="value">The translated string.</param>
	public static void Add(string culture, string cultureName, string key, string value)
	{
		if (!s_locales.TryGetValue(culture, out var locale))
		{
			locale = new Locale(culture, cultureName);
			s_locales[culture] = locale;
		}

		locale.Add(key, value);
	}

	/// <summary>
	/// Adds multiple localized strings to the specified locale.
	/// </summary>
	/// <param name="culture">The two-letter culture code of the target locale.</param>
	/// <param name="cultureName">The name of the target locale (created if missing).</param>
	/// <param name="values">The dictionary of key-value pairs to add to the locale.</param>
	public static void Add(string culture, string cultureName, Dictionary<string, string> values)
	{
		foreach ((string key, string value) in values)
		{
			Add(culture, cultureName, key, value);
		}
	}

	/// <summary>
	/// Checks if the current locale has an entry with the specified key.
	/// </summary>
	/// <param name="key">The key to check for.</param>
	/// <returns>True if the key exists in the current locale, false otherwise.</returns>
	public static bool HasString(string key) => s_currentLocale?.Get(key, out string _) ?? false;

	/// <summary>
	/// Gets a localized string for the specified key, formatted with the provided parameters.
	/// </summary>
	/// <param name="key">The key of the target translation.</param>
	/// <param name="param">
	/// An array of parameters to format the string with.
	/// It must match the number of placeholders in the localized string.
	/// </param>
	/// <returns>The formatted localized string.</returns>
	/// <exception cref="ArgumentException">Thrown if the parameter array is null.</exception>
	/// <exception cref="KeyNotFoundException">Thrown if the key is missing in the current locale.</exception>
	public static string GetStringFormatted(string key, params string[] param)
	{
		if (param == null)
			throw new ArgumentException("Parameter array cannot be null", nameof(param));

		string str = GetString(key) ?? throw new KeyNotFoundException($"Missing localized string: \"{key}\" in locale: \"{s_currentLocale?.Culture}\"");
		return string.Format(str, param);
	}

	/// <summary>
	/// Gets a string for the specified key in all available locales.
	/// </summary>
	/// <param name="key">The key of the target translation.</param>
	/// <returns>The concatenated localized strings from all locales.</returns>
	public static string GetStringAllLanguages(string key)
	{
		var builder = new StringBuilder();

		foreach ((string code, Locale locale) in s_locales)
		{
			if (locale.Get(key, out string val))
			{
				builder.AppendLine(val);
				builder.AppendLine();
			}
		}

		return builder.ToString().TrimEnd();
	}

	/// <summary>
	/// Gets a localized string for the given key from the currently active locale.
	/// </summary>
	/// <param name="key">The key of the target translation.</param>
	/// <param name="silent">A flag indicating whether to suppress warnings for missing keys.</param>
	/// <returns>The localized string if found; otherwise, an empty string.</returns>
	public static string GetString(string key, bool silent = false)
	{
		string val = string.Empty;

		if (s_currentLocale?.Get(key, out val) ?? false)
			return val;

		if (!silent)
			Log.Warning($"Missing Localized string: \"{key}\" in locale: \"{s_currentLocale?.Culture}\"");

		if (s_fallbackLocale?.Get(key, out val) ?? false)
			return val;

		if (!silent)
			Log.Error($"Missing Localized string: \"{key}\"");

		if (!silent)
		{
			string errorString = $"{{{key}}}";
			s_fallbackLocale?.Add(key, errorString);
			return errorString;
		}
		else
		{
			s_fallbackLocale?.Add(key, string.Empty);
			return string.Empty;
		}
	}

	/// <summary>
	/// Gets all entries in the current locale that start with the specified key prefix.
	/// </summary>
	/// <param name="prefix">The prefix to search for.</param>
	/// <returns>A read-only dictionary of entries with the specified key prefix.</returns>
	/// <exception cref="ArgumentException">Thrown if the provided prefix is null or empty.</exception>
	public static IReadOnlyDictionary<string, string> GetEntriesWithPrefix(string prefix)
	{
		if (string.IsNullOrEmpty(prefix))
			throw new ArgumentException("Prefix cannot be null or empty. Use GetString() instead.", nameof(prefix));

		var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (s_currentLocale == null)
			return results;

		foreach (var kvp in s_currentLocale.Translations)
		{
			if (kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				results[kvp.Key] = kvp.Value;
		}

		return results;
	}

	/// <summary>
	/// Sets the application's active locale by its two-letter culture code.
	/// </summary>
	/// <param name="locale">
	/// The two-letter culture code of the target locale (case-insensitive).
	/// </param>
	public static void SetLocale(string locale)
	{
		locale = locale.ToUpper();

		if (s_locales.TryGetValue(locale, out Locale? value))
		{
			if (s_currentLocale == value)
				return;

			s_currentLocale = value;
		}
		else
		{
			if (s_currentLocale == s_fallbackLocale)
				return;

			s_currentLocale = s_fallbackLocale;
		}

		Instance?.LocaleChanged?.Invoke();
	}

	/// <summary>
	/// Returns a disctionary of all available locales, mapping culture codes to their display names.
	/// </summary>
	/// <returns>A read-only dictionary of available locales.</returns>
	public static IReadOnlyDictionary<string, string> GetAvailableLocales()
	{
		var results = new Dictionary<string, string>(s_locales.Count, StringComparer.OrdinalIgnoreCase);
		foreach (Locale locale in s_locales.Values)
		{
			if (!results.TryAdd(locale.Culture, locale.Name))
				continue; // Duplicate; Skip
		}

		return results;
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		DependencyFactory.RegisterDependency<ILocaleProvider>(this);

		string[] languageFilePaths = EmbeddedFileUtility.GetAllFilesInDirectory("Languages");
		foreach (string languageFilePath in languageFilePaths)
		{
			try
			{
				string fileName = EmbeddedFileUtility.GetFileName(languageFilePath);
				Dictionary<string, string> values = EmbeddedFileUtility.Load<Dictionary<string, string>>(languageFilePath);

				if (!values.TryGetValue("Language", out string? name))
					name = fileName.ToUpper();

				Add(fileName, name, values);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed to load language: {languageFilePath}");
			}
		}

		s_fallbackLocale = s_locales[FALLBACK_CULTURE];
		await base.Initialize();
		SetLocale(SettingsService.Current.Language);
	}

	/// <inheritdoc/>
	bool ILocaleProvider.HasString(string key) => HasString(key);

	/// <inheritdoc/>
	string ILocaleProvider.GetStringFormatted(string key, params string[] param) => GetStringFormatted(key, param);

	/// <inheritdoc/>
	string ILocaleProvider.GetStringAllLanguages(string key) => GetStringAllLanguages(key);

	/// <inheritdoc/>
	string ILocaleProvider.GetString(string key, bool silent) => GetString(key, silent);

	/// <summary>
	/// Represents a locale with a culture and name.
	/// </summary>
	/// <param name="culture">The two-letter culture code. (e.g., "EN", "FR").</param>
	/// <param name="name">The name of the culture.</param>
	private sealed class Locale(string culture, string name)
	{
		/// <summary>Gets the locale's two-letter culture code.</summary>
		public readonly string Culture = culture;

		/// <summary>Gets the given name of the locale.</summary>
		public readonly string Name = name;

		private readonly Dictionary<string, string> translations = [];

		/// <summary>Gets all translations stored in the locale.</summary>
		public IReadOnlyDictionary<string, string> Translations => this.translations;

		/// <summary>Adds a key-value translation to the locale.</summary>
		/// <param name="key">The key of the translation.</param>
		/// <param name="value">The localized text string.</param>
		public void Add(string key, string value) => this.translations[key] = value;

		/// <summary>Gets the localized string for the given key.</summary>
		/// <param name="key">The key of the translation.</param>
		/// <param name="value">The localized text string.</param>
		/// <returns>True if the key exists in the locale, false otherwise.</returns>
		public bool Get(string key, out string value)
		{
			if (this.translations.TryGetValue(key, out string? strVal))
			{
				value = strVal;
				return true;
			}

			value = string.Empty;
			return false;
		}
	}
}
