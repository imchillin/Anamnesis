// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A service that handles the display of cycling tips in the application.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class TipService : ServiceBase<TipService>
{
	private const int TipCycleDelay = 10000; // ms (10 seconds)
	private const string MissingTipText = "I couldn't find any tips. =(";
	private List<TipEntry>? tips;
	private readonly Lock objectLock = new();

	/// <summary>
	/// Gets or sets a value indicating whether the current tip icon is Hydaelyn.
	/// </summary>
	public bool IsHydaelyn { get; private set; }

	/// <summary>
	/// Gets or sets a value indicating whether the current tip icon is Zodiark.
	/// </summary>
	public bool IsZodiark { get; private set; }

	/// <summary>
	/// Gets or sets a value indicating whether the current tip icon is Amaurotine.
	/// </summary>
	public bool IsAmaurotine { get; private set; }

	/// <summary>
	/// Gets or sets a value indicating whether the current tip icon is Anamnesis-tan (mascot).
	/// </summary>
	public bool IsAnamTan { get; private set; }

	/// <summary>
	/// Gets or sets the currently active tip.
	/// </summary>
	public TipEntry? Tip { get; private set; }

	/// <summary>
	/// Gets all tips from the current application locale.
	/// </summary>
	/// <returns>The list of tips, sorted by index.</returns>
	public static List<TipEntry> GetTipsFromLocale()
	{
		var tips = new List<TipEntry>();
		if (!LocalizationService.Loaded)
			return tips;

		var kvp = LocalizationService.GetEntriesWithPrefix("Tip_");
		var tipList = new List<(int Index, string Text, string? Url)>();
		var regex = TipKeyRegex();

		// Iterate through all translation entries with the key prefix "Tip_"
		foreach (var entry in kvp)
		{
			var match = regex.Match(entry.Key);
			if (!match.Success)
				continue;

			if (int.TryParse(match.Groups[1].Value, out int idx))
			{
				string? url = null;
				var urlKey = $"Tip_{idx}_Url";
				if (kvp.TryGetValue(urlKey, out var foundUrl))
					url = foundUrl;

				tipList.Add((idx, entry.Value, url));
			}
		}

		// Exit early if no tips were found
		if (tipList.Count == 0)
			return tips;

		// Sort by index
		tipList.Sort((a, b) => a.Index.CompareTo(b.Index));

		// Build tip entries list
		foreach (var tip in tipList)
			tips.Add(new TipEntry { Text = tip.Text, Url = tip.Url });

		return tips;
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		bool isLoaded = false;
		try
		{
			if (LocalizationService.Loaded)
			{
				lock (this.objectLock)
				{
					this.tips = GetTipsFromLocale();
					isLoaded = this.tips != null;
				}

				LocalizationService.Instance.LocaleChanged += this.OnLocaleChanged;
			}
			else
			{
				throw new Exception($"{nameof(LocalizationService)} must be loaded before initializing {nameof(TipService)}.");
			}
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to load tips.");
		}

		await base.Initialize();

		if (!isLoaded)
		{
			this.Tip = new TipEntry { Text = MissingTipText };
			return;
		}
		else
		{
			_ = Task.Run(this.TipCycle);
		}
	}

	/// <summary>
	/// Handles tips text language change on locale change.
	/// </summary>
	private void OnLocaleChanged()
	{
		lock (this.objectLock)
		{
			this.tips = GetTipsFromLocale();
		}
	}

	/// <summary>
	/// Generates a new random tip and active tip icon.
	/// </summary>
	public void NextTip()
	{
		int portraitId = Random.Shared.Next(0, 4);

		this.IsHydaelyn = portraitId == 0;
		this.IsZodiark = portraitId == 1;
		this.IsAmaurotine = portraitId == 2;
		this.IsAnamTan = portraitId == 3;

		lock (this.objectLock)
		{
			if (this.tips == null || this.tips.Count == 0)
			{
				this.Tip = new TipEntry { Text = MissingTipText };
				return;
			}

			int index = Random.Shared.Next(0, this.tips.Count);
			this.Tip = this.tips[index];
		}
	}

	/// <summary>
	/// Attempts to open the current tip's URL if present.
	/// </summary>
	public void KnowMore()
	{
		if (this.Tip?.Url == null)
			return;

		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = this.Tip.Url,
				UseShellExecute = true
			};
			Process.Start(psi);
		}
		catch (Exception ex)
		{
			Log.Warning(ex, $"Failed to open tip URL \"{this.Tip.Url}\".");
		}
	}

	private async Task TipCycle()
	{
		while (this.IsInitialized)
		{
			this.NextTip();
			await Task.Delay(TipCycleDelay);
		}
	}

	[Serializable]
	public sealed record TipEntry
	{
		public string? Text { get; init; }
		public string? Url { get; init; }

		public bool CanClick => !string.IsNullOrEmpty(this.Url);
	}

	[GeneratedRegex(@"^Tip_(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
	private static partial Regex TipKeyRegex();
}
