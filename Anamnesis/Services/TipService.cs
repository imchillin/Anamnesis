// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class TipService : ServiceBase<TipService>
{
	private const int TipCycleDelay = 10000; // ms (10 seconds)
	private List<TipEntry>? tips;

	public bool IsHydaelyn { get; set; }
	public bool IsZodiark { get; set; }
	public bool IsAmaurotine { get; set; }
	public bool IsAnamTan { get; set; }
	public TipEntry? Tip { get; set; }

	public override async Task Initialize()
	{
		try
		{
			// TODO: Move tips to localization files
			this.tips = EmbeddedFileUtility.Load<List<TipEntry>>("Data/Tips.json");
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to load tips.");
			// TODO: If failed to load this, there's no need to have a continously running task
		}

		await base.Initialize();
		_ = Task.Run(this.TipCycle);
	}

	public void NextTip()
	{
		Random rnd = new Random();

		int portraitId = rnd.Next(0, 4);

		this.IsHydaelyn = portraitId == 0;
		this.IsZodiark = portraitId == 1;
		this.IsAmaurotine = portraitId == 2;
		this.IsAnamTan = portraitId == 3;

		if (this.tips == null)
		{
			TipEntry tip = new TipEntry();
			tip.Text = "I couldn't find any tips. =(";
			this.Tip = tip;
			return;
		}

		int index = rnd.Next(0, this.tips.Count);
		this.Tip = this.tips[index];
	}

	public void KnowMore()
	{
		if (this.Tip == null || this.Tip.Url == null)
			return;

		ProcessStartInfo psi = new ProcessStartInfo();
		psi.FileName = this.Tip.Url;
		psi.UseShellExecute = true;

		Process.Start(psi);
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
	public class TipEntry
	{
		public string? Text { get; set; }
		public string? Url { get; set; }

		public bool CanClick => !string.IsNullOrEmpty(this.Url);
	}
}
