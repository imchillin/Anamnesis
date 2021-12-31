// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Pages
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Character.Utilities;
	using Anamnesis.Character.Views;
	using Anamnesis.Connect;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;
	using Serilog;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AppearancePage : UserControl
	{
		private static DirectoryInfo? lastLoadDir;
		private static DirectoryInfo? lastSaveDir;

		public AppearancePage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public GposeService GPoseService => GposeService.Instance;
		public AnamnesisConnectService AnamnesisConnectService => AnamnesisConnectService.Instance;
		public ActorRefreshService ActorRefreshService => ActorRefreshService.Instance;
		public ActorMemory? Actor { get; private set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorMemory);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!this.IsVisible)
				return;

			this.OnActorChanged(this.DataContext as ActorMemory);
		}

		private void OnClearClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			this.Actor.Equipment?.Arms?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Chest?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Ear?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Feet?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Head?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Legs?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Neck?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsPlayer);

			this.Actor?.ModelObject?.Weapons?.Hide();
			this.Actor?.ModelObject?.Weapons?.SubModel?.Hide();
		}

		private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			if (!this.Actor.IsPlayer)
			{
				this.OnClearClicked(sender, e);
				return;
			}

			this.Actor.Equipment?.Ear?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Head?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Neck?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsPlayer);
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

			if(this.Actor.Customize.Gender == ActorCustomizeMemory.Genders.Masculine)
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

			this.Actor.Equipment?.Ear?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Head?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Neck?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsPlayer);
			this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsPlayer);
		}

		private async void OnLoadClicked(object sender, RoutedEventArgs e)
		{
			await this.Load(CharacterFile.SaveModes.All);
		}

		private async void OnLoadEquipmentClicked(object sender, RoutedEventArgs e)
		{
			await this.Load(CharacterFile.SaveModes.Equipment);
		}

		private async void OnLoadGearClicked(object sender, RoutedEventArgs e)
		{
			await this.Load(CharacterFile.SaveModes.EquipmentGear);
		}

		private async void OnLoadAccessoriesClicked(object sender, RoutedEventArgs e)
		{
			await this.Load(CharacterFile.SaveModes.EquipmentAccessories);
		}

		private async void OnLoadAppearanceClicked(object sender, RoutedEventArgs e)
		{
			await this.Load(CharacterFile.SaveModes.Appearance);
		}

		private async void OnLoadWeaponsClicked(object sender, RoutedEventArgs e)
		{
			await this.Load(CharacterFile.SaveModes.EquipmentWeapons);
		}

		private void OnLoadNpcClicked(object sender, RoutedEventArgs e)
		{
			this.LoadNpc(CharacterFile.SaveModes.All);
		}

		private void OnLoadNpcEquipmentClicked(object sender, RoutedEventArgs e)
		{
			this.LoadNpc(CharacterFile.SaveModes.Equipment);
		}

		private void OnLoadNpcAppearanceClicked(object sender, RoutedEventArgs e)
		{
			this.LoadNpc(CharacterFile.SaveModes.Appearance);
		}

		private void OnLoadNpcWeaponsClicked(object sender, RoutedEventArgs e)
		{
			this.LoadNpc(CharacterFile.SaveModes.EquipmentWeapons);
		}

		private void LoadNpc(CharacterFile.SaveModes mode)
		{
			SelectorDrawer.Show<NpcSelector, INpcBase>(null, (npc) =>
			{
				if (npc == null)
					return;

				Task.Run(() => this.ApplyNpc(npc, mode));
			});
		}

		private void OnLoadObjectClicked(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<ModelListSelector, ModelListEntry>(null, (npc) =>
			{
				if (npc == null)
					return;

				Task.Run(() => this.ApplyNpc(npc, CharacterFile.SaveModes.Appearance));
			});
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

		private async Task Load(CharacterFile.SaveModes mode)
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
				Log.Error(ex, "Failed to load appearance");
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.Save();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to save appearance");
			}
		}

		private async Task Save()
		{
			if (this.Actor == null)
				return;

			lastSaveDir = await CharacterFile.Save(lastSaveDir, this.Actor);
		}

		private void OnActorChanged(ActorMemory? actor)
		{
			this.Actor = actor;

			Application.Current.Dispatcher.InvokeAsync(() =>
			{
				bool hasValidSelection = actor != null && (actor.ObjectKind == ActorTypes.Player || actor.ObjectKind == ActorTypes.BattleNpc || actor.ObjectKind == ActorTypes.EventNpc);
				this.IsEnabled = hasValidSelection;
			});
		}
	}
}
