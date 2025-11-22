// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Pages;

using Anamnesis.Actor.Utilities;
using Anamnesis.Actor.Views;
using Anamnesis.Core.Extensions;
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
using System.Windows.Input;
using System.Windows.Navigation;
using static Anamnesis.Actor.Utilities.DyeUtility;

/// <summary>
/// Interaction logic for AppearancePage.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class CharacterPage : UserControl
{
	private static DirectoryInfo? s_lastLoadDir;
	private static DirectoryInfo? s_lastSaveDir;

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

	public static PoseService PoseService => PoseService.Instance;

	public ObjectHandle<ActorMemory>? Actor { get; private set; }
	public ListCollectionView VoiceEntries { get; private set; }
	public bool CanDyeEquipment => this.Actor != null && this.Actor.Do(a => a.Equipment != null);

	public bool CanDyeWeapons
	{
		get
		{
			if (this.Actor == null)
				return false;

			// One of the weapons must not be null and have a non-zero set to be dyable.
			return this.Actor.Do(a =>
				(a.MainHand != null && a.MainHand.Set != 0) ||
				(a.OffHand != null && a.OffHand.Set != 0));
		}
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
					VoiceId = voiceId,
				};

				entries.Add(entry);
			}
		}

		var voices = new ListCollectionView(entries);
		voices.GroupDescriptions.Add(new PropertyGroupDescription("VoiceCategory"));
		return voices;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ObjectHandle<ActorMemory>);
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ObjectHandle<ActorMemory>);
	}

	private void OnClearClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		if (this.Actor == null)
			return;

		this.Actor.Do(a =>
		{
			bool isHuman = a.IsHuman;

			a.MainHand?.Clear(isHuman);
			a.OffHand?.Clear(isHuman);

			if (a.Equipment != null)
			{
				a.Equipment.Arms?.Clear(isHuman);
				a.Equipment.Chest?.Clear(isHuman);
				a.Equipment.Ear?.Clear(isHuman);
				a.Equipment.Feet?.Clear(isHuman);
				a.Equipment.Head?.Clear(isHuman);
				a.Equipment.Legs?.Clear(isHuman);
				a.Equipment.LFinger?.Clear(isHuman);
				a.Equipment.Neck?.Clear(isHuman);
				a.Equipment.RFinger?.Clear(isHuman);
				a.Equipment.Wrist?.Clear(isHuman);
			}

			a.Glasses?.Clear();

			if (a.ModelObject != null)
			{
				if (a.ModelObject.ChildObject != null)
					a.ModelObject.ChildObject.IsVisible = false;

				if (a.ModelObject.ChildObject?.NextSiblingObject != null)
					a.ModelObject.ChildObject.NextSiblingObject.IsVisible = false;
			}
		});
	}

	private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		this.Actor.Do(a =>
		{
			bool isHuman = a.IsHuman;
			if (!isHuman)
			{
				this.OnClearClicked(sender, e);
				return;
			}

			a.MainHand?.Clear(isHuman);
			a.OffHand?.Clear(isHuman);

			if (a.Equipment != null)
			{
				a.Equipment.Ear?.Clear(isHuman);
				a.Equipment.Head?.Clear(isHuman);
				a.Equipment.LFinger?.Clear(isHuman);
				a.Equipment.Neck?.Clear(isHuman);
				a.Equipment.RFinger?.Clear(isHuman);
				a.Equipment.Wrist?.Clear(isHuman);
				a.Equipment.Arms?.Equip(ItemUtility.NpcBodyItem);
				a.Equipment.Chest?.Equip(ItemUtility.NpcBodyItem);
				a.Equipment.Legs?.Equip(ItemUtility.NpcBodyItem);
				a.Equipment.Feet?.Equip(ItemUtility.NpcBodyItem);
			}

			a.Glasses?.Clear();
		});
	}

	private void OnEmperorsNewGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		this.Actor.Do(a =>
		{
			bool isHuman = a.IsHuman;
			if (!isHuman)
			{
				this.OnClearClicked(sender, e);
				return;
			}

			a.MainHand?.Clear(isHuman);
			a.OffHand?.Clear(isHuman);

			if (a.Equipment != null)
			{
				a.Equipment.Ear?.Equip(ItemUtility.EmperorsAccessoryItem);
				a.Equipment.Neck?.Equip(ItemUtility.EmperorsAccessoryItem);
				a.Equipment.Wrist?.Equip(ItemUtility.EmperorsAccessoryItem);
				a.Equipment.LFinger?.Equip(ItemUtility.EmperorsAccessoryItem);
				a.Equipment.RFinger?.Equip(ItemUtility.EmperorsAccessoryItem);
				a.Equipment.Head?.Equip(ItemUtility.EmperorsBodyItem);
				a.Equipment.Chest?.Equip(ItemUtility.EmperorsBodyItem);
				a.Equipment.Arms?.Equip(ItemUtility.EmperorsBodyItem);
				a.Equipment.Legs?.Equip(ItemUtility.EmperorsBodyItem);
				a.Equipment.Feet?.Equip(ItemUtility.EmperorsBodyItem);
			}

			a.Glasses?.Clear();
		});
	}

	private void OnRaceGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		this.Actor.Do(a =>
		{
			if (a.Customize?.Race == null)
				return;

			var race = GameDataService.Races.GetRow((uint)a.Customize.Race);

			if (a.Equipment != null)
			{
				if (a.Customize.Gender == ActorCustomizeMemory.Genders.Masculine)
				{
					a.Equipment.Chest?.Equip(race.RSEMBody.Value);
					a.Equipment.Arms?.Equip(race.RSEMHands.Value);
					a.Equipment.Legs?.Equip(race.RSEMLegs.Value);
					a.Equipment.Feet?.Equip(race.RSEMFeet.Value);
				}
				else
				{
					a.Equipment.Chest?.Equip(race.RSEFBody.Value);
					a.Equipment.Arms?.Equip(race.RSEFHands.Value);
					a.Equipment.Legs?.Equip(race.RSEFLegs.Value);
					a.Equipment.Feet?.Equip(race.RSEFFeet.Value);
				}

				bool isHuman = a.IsHuman;
				a.Equipment.Ear?.Clear(isHuman);
				a.Equipment.Head?.Clear(isHuman);
				a.Equipment.LFinger?.Clear(isHuman);
				a.Equipment.Neck?.Clear(isHuman);
				a.Equipment.RFinger?.Clear(isHuman);
				a.Equipment.Wrist?.Clear(isHuman);
			}

			a.Glasses?.Clear();
		});
	}

	// Dye1 - ALL equipment
	private void OnSetDye1OnAllEquipmentClicked(object sender, RoutedEventArgs e)
	{
		this.ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget.All, DyeSlot.First);
	}

	// Dye1 - Weapons only - Will dye both weapons if able.
	private void OnSetDye1OnWeaponsClicked(object sender, RoutedEventArgs e)
	{
		this.ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget.Weapons, DyeSlot.First);
	}

	// Dye1 - Clothes (left side) only.
	private void OnSetDye1OnClothesClicked(object sender, RoutedEventArgs e)
	{
		this.ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget.Clothing, DyeSlot.First);
	}

	// Dye1 - Accessories (right side) only.
	private void OnSetDye1OnAccessoriesClicked(object sender, RoutedEventArgs e)
	{
		this.ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget.Accessories, DyeSlot.First);
	}

	// Dye1 - Clear on ALL equipment
	private void OnClearDye1OnAllEquipmentClicked(object sender, RoutedEventArgs e)
	{
		this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.All, DyeSlot.First);
	}

	// Dye1 - Clear on weapons only.
	private void OnClearDye1OnWeaponsClicked(object sender, RoutedEventArgs e)
	{
		this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.Weapons, DyeSlot.First);
	}

	// Dye1 - Clear on clothes (left side) only.
	private void OnClearDye1OnClothesClicked(object sender, RoutedEventArgs e)
	{
		this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.Clothing, DyeSlot.First);
	}

	// Dye1 - Clear on accessories (right side) only.
	private void OnClearDye1OnAccessoriesClicked(object sender, RoutedEventArgs e)
	{
		this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.Accessories, DyeSlot.First);
	}

	// Dye1 - Middle click to clear - All Equipment
	private void OnSetDye1OnAllEquipmentMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
		{
			this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.All, DyeSlot.First);
		}
	}

	// Dye2 - ALL Equipment
	private void OnSetDye2OnAllEquipmentClicked(object sender, RoutedEventArgs e)
	{
		this.ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget.All, DyeSlot.Second);
	}

	// Dye2 - Weapons only - Will dye both weapons if able.
	private void OnSetDye2OnWeaponsClicked(object sender, RoutedEventArgs e)
	{
		this.ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget.Weapons, DyeSlot.Second);
	}

	// Dye2 - Clothes (left side) only.
	private void OnSetDye2OnClothesClicked(object sender, RoutedEventArgs e)
	{
		this.ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget.Clothing, DyeSlot.Second);
	}

	// Dye2 - Accessories (right side) only.
	private void OnSetDye2OnAccessoriesClicked(object sender, RoutedEventArgs e)
	{
		this.ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget.Accessories, DyeSlot.Second);
	}

	// Dye2 - Clear on ALL equipment
	private void OnClearDye2OnAllEquipmentClicked(object sender, RoutedEventArgs e)
	{
		this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.All, DyeSlot.Second);
	}

	// Dye2 - Clear on weapons only.
	private void OnClearDye2OnWeaponsClicked(object sender, RoutedEventArgs e)
	{
		this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.Weapons, DyeSlot.Second);
	}

	// Dye2 - Clear on clothes (left side) only.
	private void OnClearDye2OnClothesClicked(object sender, RoutedEventArgs e)
	{
		this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.Clothing, DyeSlot.Second);
	}

	// Dye2 - Clear on accessories (right side) only.
	private void OnClearDye2OnAccessoriesClicked(object sender, RoutedEventArgs e)
	{
		this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.Accessories, DyeSlot.Second);
	}

	// Dye2 - Middle click to clear - All Equipment
	private void OnSetDye2OnAllEquipmentMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
		{
			this.ApplyDyeToEquipment(DyeUtility.NoneDye, DyeTarget.All, DyeSlot.Second);
		}
	}

	// Show dye selection drawer, and then apply the resulting dye as specified.
	// If the actor is null or we are unable to dye anything, return.
	// Otherwise, check each intended dye target and remove them if they cannot be dyed.
	// Show the drawer, and apply the dye to the remaining valid targets.
	private void ShowDyeDrawerAndApplyDyeToEquipment(DyeTarget dyeTarget, DyeSlot dyeSlot)
	{
		if (this.Actor == null)
			return;

		if (!this.CanDyeWeapons && !this.CanDyeEquipment)
			return;

		if (dyeTarget.HasFlagUnsafe(DyeTarget.Weapons) && !this.CanDyeWeapons)
			dyeTarget &= ~DyeTarget.Weapons;

		if (dyeTarget.HasFlagUnsafe(DyeTarget.Clothing) && !this.CanDyeEquipment)
			dyeTarget &= ~DyeTarget.Clothing;

		if (dyeTarget.HasFlagUnsafe(DyeTarget.Accessories) && !this.CanDyeEquipment)
			dyeTarget &= ~DyeTarget.Accessories;

		SelectorDrawer.Show<DyeSelector, IDye>(null, (dye) =>
		{
			this.ApplyDyeToEquipment(dye, dyeTarget, dyeSlot);
		});
	}

	// Applies a dye to a specified slot on the specified targets, whether they be weapons or clothing.
	private void ApplyDyeToEquipment(IDye dye, DyeTarget dyeTarget, DyeSlot dyeSlot)
	{
		if (this.Actor == null || dye == null)
			return;

		this.Actor.Do(a =>
		{
			if (this.CanDyeWeapons && dyeTarget.HasFlagUnsafe(DyeTarget.Weapons))
				a.MainHand?.ApplyDye(dye, dyeSlot);

			if (this.CanDyeWeapons && dyeTarget.HasFlagUnsafe(DyeTarget.Weapons))
				a.OffHand?.ApplyDye(dye, dyeSlot);

			if (this.CanDyeEquipment && dyeTarget.HasFlagUnsafe(DyeTarget.Clothing))
			{
				a.Equipment?.Head?.ApplyDye(dye, dyeSlot);
				a.Equipment?.Chest?.ApplyDye(dye, dyeSlot);
				a.Equipment?.Arms?.ApplyDye(dye, dyeSlot);
				a.Equipment?.Legs?.ApplyDye(dye, dyeSlot);
				a.Equipment?.Feet?.ApplyDye(dye, dyeSlot);
			}

			if (this.CanDyeEquipment && dyeTarget.HasFlagUnsafe(DyeTarget.Accessories))
			{
				a.Equipment?.Ear?.ApplyDye(dye, dyeSlot);
				a.Equipment?.Neck?.ApplyDye(dye, dyeSlot);
				a.Equipment?.Wrist?.ApplyDye(dye, dyeSlot);
				a.Equipment?.RFinger?.ApplyDye(dye, dyeSlot);
				a.Equipment?.LFinger?.ApplyDye(dye, dyeSlot);
			}
		});
	}

	private async void OnResetClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		var pinned = TargetService.GetPinned(this.Actor);
		if (pinned == null)
			return;

		if (pinned.OriginalCharacterBackup == null)
			return;

		if (await GenericDialog.ShowLocalizedAsync("Character_Reset_Confirm", "Character_Reset", MessageBoxButton.YesNo) != true)
			return;

		pinned.RestoreCharacterBackup(PinnedActor.BackupModes.Original);
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
			Shortcut[]? shortcuts =
			[
				FileService.DefaultCharacterDirectory,
				FileService.StandardAppearancesDirectory,
				FileService.FFxivDatCharacterDirectory,
				FileService.CMToolAppearanceSaveDir,
			];

			Type[] types =
			[
				typeof(CmToolAppearanceFile),
				typeof(CmToolAppearanceJsonFile),
				typeof(CmToolGearsetFile),
				typeof(CmToolLegacyAppearanceFile),
				typeof(DatCharacterFile),
				typeof(CharacterFile),
			];

			OpenResult result = await FileService.Open(s_lastLoadDir, shortcuts, types);

			if (result.File == null)
				return;

			s_lastLoadDir = result.Directory;

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

		SaveResult result = await FileService.Save<CharacterFile>(s_lastSaveDir, FileService.DefaultCharacterDirectory);

		if (result.Path == null)
			return;

		var file = new CharacterFile();
		file.WriteToFile(this.Actor, mode);

		using var stream = new FileStream(result.Path.FullName, FileMode.Create);
		file.Serialize(stream);

		s_lastSaveDir = result.Directory;

		if (editMeta)
		{
			FileMetaEditor.Show(result.Path, file);
		}
	}

	private async Task SaveDat()
	{
		if (this.Actor == null)
			return;

		SaveResult result = await FileService.Save<DatCharacterFile>(s_lastSaveDir, FileService.DefaultCharacterDirectory);

		if (result.Path == null)
			return;

		var file = new DatCharacterFile();
		file.WriteToFile(this.Actor);

		using var stream = new FileStream(result.Path.FullName, FileMode.Create);
		file.Serialize(stream);

		s_lastSaveDir = result.Directory;
	}

	private void OnActorChanged(ObjectHandle<ActorMemory>? actorHandle)
	{
		this.Actor = actorHandle;

		Application.Current.Dispatcher.InvokeAsync(() =>
		{
			bool hasValidSelection = actorHandle != null && actorHandle.Do(a => a.ObjectKind.IsSupportedType());
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
