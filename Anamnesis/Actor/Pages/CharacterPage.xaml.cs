// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Pages;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Anamnesis.Actor.Utilities;
using Anamnesis.Actor.Views;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles;
using Anamnesis.Styles.Drawers;
using Anamnesis.Windows;
using PropertyChanged;
using Serilog;

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

		this.VoiceEntries = this.GenerateVoiceList();
	}

	public ActorMemory? Actor { get; private set; }
	public ListCollectionView VoiceEntries { get; private set; }

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ActorMemory);
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ActorMemory);
	}

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
	}

	private void OnRaceGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (this.Actor.Customize?.Race == null)
			return;

		var race = GameDataService.Races.GetRow((uint)this.Actor.Customize.Race);

		if (race == null)
			return;

		if (this.Actor.Customize.Gender == ActorCustomizeMemory.Genders.Masculine)
		{
			var body = GameDataService.Items.Get((uint)race.RSEMBody);
			var hands = GameDataService.Items.Get((uint)race.RSEMHands);
			var legs = GameDataService.Items.Get((uint)race.RSEMLegs);
			var feet = GameDataService.Items.Get((uint)race.RSEMFeet);

			this.Actor.Equipment?.Chest?.Equip(body);
			this.Actor.Equipment?.Arms?.Equip(hands);
			this.Actor.Equipment?.Legs?.Equip(legs);
			this.Actor.Equipment?.Feet?.Equip(feet);
		}
		else
		{
			var body = GameDataService.Items.Get((uint)race.RSEFBody);
			var hands = GameDataService.Items.Get((uint)race.RSEFHands);
			var legs = GameDataService.Items.Get((uint)race.RSEFLegs);
			var feet = GameDataService.Items.Get((uint)race.RSEFFeet);

			this.Actor.Equipment?.Chest?.Equip(body);
			this.Actor.Equipment?.Arms?.Equip(hands);
			this.Actor.Equipment?.Legs?.Equip(legs);
			this.Actor.Equipment?.Feet?.Equip(feet);
		}

		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);
	}

	private async void OnResetClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor?.Pinned?.OriginalCharacterBackup == null)
			return;

		if (await GenericDialog.ShowLocalizedAsync("Character_Reset_Confirm", "Character_Reset", MessageBoxButton.YesNo) != true)
			return;

		await this.Actor.Pinned.RestoreCharacterBackup(PinnedActor.BackupModes.Original);
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
		throw new NotImplementedException();
		/*SelectorControl.Show<NpcSelector, INpcBase>(null, (npc) =>
		{
			if (npc == null)
				return;

			Task.Run(() => this.ApplyNpc(npc, mode));
		});*/
	}

	private async Task ApplyNpc(INpcBase? npc, CharacterFile.SaveModes mode = CharacterFile.SaveModes.All)
	{
		if (this.Actor == null || npc == null)
			return;

		try
		{
			CharacterFile apFile = npc.ToFile();
			await apFile.Apply(this.Actor, mode);
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
				await apFile.Apply(this.Actor, mode);
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

	public class VoiceEntry
	{
		public byte VoiceId { get; set; }
		public string VoiceName { get; set; } = string.Empty;
		public string VoiceCategory { get; set; } = string.Empty;
	}
}
