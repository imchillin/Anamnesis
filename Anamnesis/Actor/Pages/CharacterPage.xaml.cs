// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Pages;

using Anamnesis.Actor.Utilities;
using Anamnesis.Actor.Views;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;

/// <summary>
/// Interaction logic for AppearancePage.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class CharacterPage : UserControl
{
	private static DirectoryInfo? lastLoadDir;
	private static DirectoryInfo? lastSaveDir;

	public CharacterPage()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		HotkeyService.RegisterHotkeyHandler("CharacterPage.ClearEquipment", () => this.OnClearClicked());

		Stopwatch stopwatch = Stopwatch.StartNew();
		this.VoiceEntries = GenerateVoiceList();
		stopwatch.Stop();
		Log.Verbose($"Voice list generation time: {stopwatch.ElapsedMilliseconds} ms");
	}

	public ActorMemory? Actor { get; private set; }
	public ListCollectionView VoiceEntries { get; private set; }
	public PoseService PoseService => PoseService.Instance;

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ActorMemory);
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ActorMemory);
	}

	private static ListCollectionView GenerateVoiceList()
	{
		List<VoiceEntry> entries = [];
		foreach (var makeType in GameDataService.CharacterMakeTypes)
		{
			if (makeType.Tribe.RowId == 0)
				continue;

			Tribe? tribe = makeType.Tribe.Value;

			if (tribe == null)
				continue;

			int voiceCount = makeType.Voices!.Count;
			for (int i = 0; i < voiceCount; i++)
			{
				byte voiceId = makeType.Voices[i]!;
				VoiceEntry entry = new()
				{
					VoiceName = $"Voice #{i + 1} ({voiceId})",
					VoiceCategory = $"{makeType.Race.Value.Name}, {tribe.Value.Masculine} ({makeType.Gender})",
					VoiceId = voiceId
				};

				entries.Add(entry);
			}
		}

		ListCollectionView voices = new ListCollectionView(entries);
		voices.GroupDescriptions.Add(new PropertyGroupDescription("VoiceCategory"));
		return voices;
	}

	private void OnClearClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		if (this.Actor == null)
			return;

		this.Actor.MainHand?.Clear(this.Actor.IsHuman);
		this.Actor.OffHand?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Arms?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Chest?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Feet?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Legs?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);
		this.Actor.Glasses?.Clear();

		this.Actor?.ModelObject?.Weapons?.Hide();
		this.Actor?.ModelObject?.Weapons?.SubModel?.Hide();
	}

	private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (!this.Actor.IsHuman)
		{
			this.OnClearClicked(sender, e);
			return;
		}

		this.Actor.MainHand?.Clear(this.Actor.IsHuman);
		this.Actor.OffHand?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Arms?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Chest?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Legs?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Feet?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Glasses?.Clear();
	}

	private void OnEmperorsNewGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (!this.Actor.IsHuman)
		{
			this.OnClearClicked(sender, e);
			return;
		}

		this.Actor.MainHand?.Clear(this.Actor.IsHuman);
		this.Actor.OffHand?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Ear?.Equip(ItemUtility.EmperorsAccessoryItem);
		this.Actor.Equipment?.Neck?.Equip(ItemUtility.EmperorsAccessoryItem);
		this.Actor.Equipment?.Wrist?.Equip(ItemUtility.EmperorsAccessoryItem);
		this.Actor.Equipment?.LFinger?.Equip(ItemUtility.EmperorsAccessoryItem);
		this.Actor.Equipment?.RFinger?.Equip(ItemUtility.EmperorsAccessoryItem);
		this.Actor.Equipment?.Head?.Equip(ItemUtility.EmperorsBodyItem);
		this.Actor.Equipment?.Chest?.Equip(ItemUtility.EmperorsBodyItem);
		this.Actor.Equipment?.Arms?.Equip(ItemUtility.EmperorsBodyItem);
		this.Actor.Equipment?.Legs?.Equip(ItemUtility.EmperorsBodyItem);
		this.Actor.Equipment?.Feet?.Equip(ItemUtility.EmperorsBodyItem);
		this.Actor.Glasses?.Clear();
	}

	private void OnRaceGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (this.Actor.Customize?.Race == null)
			return;

		var race = GameDataService.Races.GetRow((uint)this.Actor.Customize.Race);

		if (this.Actor.Customize.Gender == ActorCustomizeMemory.Genders.Masculine)
		{
			this.Actor.Equipment?.Chest?.Equip(race.RSEMBody.Value);
			this.Actor.Equipment?.Arms?.Equip(race.RSEMHands.Value);
			this.Actor.Equipment?.Legs?.Equip(race.RSEMLegs.Value);
			this.Actor.Equipment?.Feet?.Equip(race.RSEMFeet.Value);
		}
		else
		{
			this.Actor.Equipment?.Chest?.Equip(race.RSEFBody.Value);
			this.Actor.Equipment?.Arms?.Equip(race.RSEFHands.Value);
			this.Actor.Equipment?.Legs?.Equip(race.RSEFLegs.Value);
			this.Actor.Equipment?.Feet?.Equip(race.RSEFFeet.Value);
		}

		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);
		this.Actor.Glasses?.Clear();
	}

	private async void OnResetClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor?.Pinned?.OriginalCharacterBackup == null)
			return;

		if (await GenericDialog.ShowLocalizedAsync("Character_Reset_Confirm", "Character_Reset", MessageBoxButton.YesNo) != true)
			return;

		this.Actor.Pinned.RestoreCharacterBackup(PinnedActor.BackupModes.Original);
	}

	private async void OnImportClicked(object sender, RoutedEventArgs e)
	{
		await this.ImportCharacter(CharacterFile.SaveModes.All);
	}

	private async void OnImportEquipmentClicked(object sender, RoutedEventArgs e)
	{
		await this.ImportCharacter(CharacterFile.SaveModes.Equipment);
	}

	private async void OnImportGearClicked(object sender, RoutedEventArgs e)
	{
		await this.ImportCharacter(CharacterFile.SaveModes.EquipmentGear);
	}

	private async void OnImportAccessoriesClicked(object sender, RoutedEventArgs e)
	{
		await this.ImportCharacter(CharacterFile.SaveModes.EquipmentAccessories);
	}

	private async void OnImportAppearanceClicked(object sender, RoutedEventArgs e)
	{
		await this.ImportCharacter(CharacterFile.SaveModes.Appearance);
	}

	private async void OnImportWeaponsClicked(object sender, RoutedEventArgs e)
	{
		await this.ImportCharacter(CharacterFile.SaveModes.EquipmentWeapons);
	}

	private void OnImportNpcClicked(object sender, RoutedEventArgs e)
	{
		this.ImportNpc(CharacterFile.SaveModes.All);
	}

	private void OnImportNpcEquipmentClicked(object sender, RoutedEventArgs e)
	{
		this.ImportNpc(CharacterFile.SaveModes.Equipment);
	}

	private void OnImportNpcAppearanceClicked(object sender, RoutedEventArgs e)
	{
		this.ImportNpc(CharacterFile.SaveModes.Appearance);
	}

	private void OnImportNpcWeaponsClicked(object sender, RoutedEventArgs e)
	{
		this.ImportNpc(CharacterFile.SaveModes.EquipmentWeapons);
	}

	private void ImportNpc(CharacterFile.SaveModes mode)
	{
		SelectorDrawer.Show<NpcSelector, INpcBase>(null, (npc) =>
		{
			if (npc == null)
				return;

			this.ApplyNpc(npc, mode);
		});
	}

	private void ApplyNpc(INpcBase? npc, CharacterFile.SaveModes mode = CharacterFile.SaveModes.All)
	{
		if (this.Actor == null || npc == null)
			return;

		try
		{
			CharacterFile apFile = npc.ToFile();
			apFile.Apply(this.Actor, mode);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to load NPC appearance");
		}
	}

	private async Task ImportCharacter(CharacterFile.SaveModes mode)
	{
		if (this.Actor == null)
			return;

		try
		{
			Shortcut[]? shortcuts = new[]
			{
				FileService.DefaultCharacterDirectory,
				FileService.FFxivDatCharacterDirectory,
				FileService.CMToolAppearanceSaveDir,
			};

			Type[] types = new[]
			{
				typeof(CmToolAppearanceFile),
				typeof(CmToolAppearanceJsonFile),
				typeof(CmToolGearsetFile),
				typeof(CmToolLegacyAppearanceFile),
				typeof(DatCharacterFile),
				typeof(CharacterFile),
			};

			OpenResult result = await FileService.Open(lastLoadDir, shortcuts, types);

			if (result.File == null)
				return;

			lastLoadDir = result.Directory;

			if (result.File is IUpgradeCharacterFile legacyFile)
				result.File = legacyFile.Upgrade();

			if (result.File is CharacterFile apFile)
			{
				apFile.Apply(this.Actor, mode);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to export character file");
		}
	}

	private async void OnExportClicked(object sender, RoutedEventArgs e)
	{
		try
		{
			await this.Save(false);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to export character file");
		}
	}

	private async void OnExportMetaClicked(object sender, RoutedEventArgs e)
	{
		try
		{
			await this.Save(true);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to export character file");
		}
	}

	private async void OnExportDatClicked(object sender, RoutedEventArgs e)
	{
		try
		{
			await this.SaveDat();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to export character dat file");
		}
	}

	private async Task Save(bool editMeta, CharacterFile.SaveModes mode = CharacterFile.SaveModes.All)
	{
		if (this.Actor == null)
			return;

		SaveResult result = await FileService.Save<CharacterFile>(lastSaveDir, FileService.DefaultCharacterDirectory);

		if (result.Path == null)
			return;

		CharacterFile file = new CharacterFile();
		file.WriteToFile(this.Actor, mode);

		using FileStream stream = new FileStream(result.Path.FullName, FileMode.Create);
		file.Serialize(stream);

		lastSaveDir = result.Directory;

		if (editMeta)
		{
			FileMetaEditor.Show(result.Path, file);
		}
	}

	private async Task SaveDat()
	{
		if (this.Actor == null)
			return;

		SaveResult result = await FileService.Save<DatCharacterFile>(lastSaveDir, FileService.DefaultCharacterDirectory);

		if (result.Path == null)
			return;

		DatCharacterFile file = new DatCharacterFile();
		file.WriteToFile(this.Actor);

		using FileStream stream = new FileStream(result.Path.FullName, FileMode.Create);
		file.Serialize(stream);

		lastSaveDir = result.Directory;
	}

	private void OnActorChanged(ActorMemory? actor)
	{
		this.Actor = actor;

		Application.Current.Dispatcher.InvokeAsync(() =>
		{
			bool hasValidSelection = actor != null && actor.ObjectKind.IsSupportedType();
			this.IsEnabled = hasValidSelection;
		});
	}

	private void OnNavigate(object sender, RequestNavigateEventArgs e)
	{
		UrlUtility.Open(e.Uri.AbsoluteUri);
	}

	public class VoiceEntry
	{
		public byte VoiceId { get; set; }
		public string VoiceName { get; set; } = string.Empty;
		public string VoiceCategory { get; set; } = string.Empty;
	}
}
