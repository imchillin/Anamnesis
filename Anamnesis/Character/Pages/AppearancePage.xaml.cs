// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Pages
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Character.Dialogs;
	using Anamnesis.Character.Views;
	using Anamnesis.Files;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Anamnesis.WpfStyles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class AppearancePage : UserControl
	{
		public AppearancePage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public GposeService GPoseService => GposeService.Instance;
		public ActorViewModel? Actor { get; private set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorViewModel);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorViewModel);
		}

		private async void OnLoadClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.Load();
			}
			catch (Exception ex)
			{
				Log.Write(ex, "Appearance", Log.Severity.Error);
			}
		}

		private void OnLoadNpcClicked(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<NpcSelector, INpcResident>("Select NPC", null, (v) => { this.ApplyNpc(v?.Appearance); });
		}

		private async void ApplyNpc(INpcBase? npc)
		{
			if (this.Actor == null || npc == null)
				return;

			CharacterFile apFile = npc.ToFile();
			await apFile.Apply(this.Actor, CharacterFile.SaveModes.All);
		}

		private async Task Load()
		{
			if (this.Actor == null)
				return;

			FileBase? file = await FileService.Open<LegacyEquipmentSetFile, LegacyCharacterFile, DatCharacterFile, CharacterFile>();

			if (file == null)
				return;

			bool advanced = true;

			if (file is LegacyCharacterFile legacyAllFile)
				file = legacyAllFile.Upgrade();

			if (file is LegacyEquipmentSetFile legacyEquipmentFile)
				file = legacyEquipmentFile.Upgrade();

			if (file is DatCharacterFile datFile)
				file = datFile.Upgrade();

			if (file is CharacterFile apFile)
			{
				CharacterFile.SaveModes mode = CharacterFile.SaveModes.All;

				if (advanced)
				{
					mode = await ViewService.ShowDialog<AppearanceModeSelectorDialog, CharacterFile.SaveModes>("Load Appearance...");

					if (mode == CharacterFile.SaveModes.None)
					{
						return;
					}
				}

				await apFile.Apply(this.Actor, mode);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			if (this.Actor == null)
				return;

			await FileService.Save(
				async (advancedMode) =>
				{
					CharacterFile.SaveModes mode = CharacterFile.SaveModes.All;

					if (advancedMode)
						mode = await ViewService.ShowDialog<AppearanceModeSelectorDialog, CharacterFile.SaveModes>("Save Appearance...");

					CharacterFile file = new CharacterFile();

					if (mode == CharacterFile.SaveModes.None)
						return null;

					file.Read(this.Actor, mode);

					return file;
				});
		}

		private void OnActorChanged(ActorViewModel? actor)
		{
			this.Actor = actor;
			bool hasValidSelection = actor != null && (actor.ObjectKind == ActorTypes.Player || actor.ObjectKind == ActorTypes.BattleNpc || actor.ObjectKind == ActorTypes.EventNpc);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = hasValidSelection;
			});
		}
	}
}
