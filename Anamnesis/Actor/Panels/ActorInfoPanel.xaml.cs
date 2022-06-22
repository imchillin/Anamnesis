// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.GameData.Excel;
using Anamnesis.Panels;
using Anamnesis.Services;
using System.Collections.Generic;
using System.Windows.Data;

public partial class ActorInfoPanel : ActorPanelBase
{
	public ActorInfoPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.VoiceEntries = this.GenerateVoiceList();
	}

	public ListCollectionView VoiceEntries { get; private set; }

	private ListCollectionView GenerateVoiceList()
	{
		List<VoiceEntry> entries = new();
		foreach (var makeType in GameDataService.CharacterMakeTypes)
		{
			if (makeType == null)
				continue;

			if (makeType.Tribe == 0)
				continue;

			Tribe? tribe = GameDataService.Tribes.GetRow((uint)makeType.Tribe);

			if (tribe == null)
				continue;

			int voiceCount = makeType.Voices!.Count;
			for (int i = 0; i < voiceCount; i++)
			{
				byte voiceId = makeType.Voices[i]!;
				VoiceEntry entry = new();
				entry.VoiceName = $"Voice #{i + 1} ({voiceId})";
				entry.VoiceCategory = $"{makeType.Race}, {tribe.Masculine} ({makeType.Gender})";
				entry.VoiceId = voiceId;
				entries.Add(entry);
			}
		}

		ListCollectionView voices = new ListCollectionView(entries);
		voices.GroupDescriptions.Add(new PropertyGroupDescription("VoiceCategory"));
		return voices;
	}

	public class VoiceEntry
	{
		public byte VoiceId { get; set; }
		public string VoiceName { get; set; } = string.Empty;
		public string VoiceCategory { get; set; } = string.Empty;
	}
}
