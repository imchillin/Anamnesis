// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.AppearanceModule.Dialogs;
	using Anamnesis.AppearanceModule.Files;
	using Anamnesis.AppearanceModule.Views;
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

		public bool IsOverworld { get; private set; }
		public ActorViewModel Actor { get; private set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			TargetService.ModeChanged += this.SelectionModeChanged;
			this.OnActorChanged(this.DataContext as ActorViewModel);
			this.SelectionModeChanged(TargetService.CurrentMode);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorViewModel);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			TargetService.ModeChanged -= this.SelectionModeChanged;
		}

		private async void OnLoadClicked(object sender, RoutedEventArgs e)
		{
			await this.Load();
		}

		private void OnLoadNpcClicked(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<NpcSelector, INpcResident>("Select NPC", null, (v) => { this.ApplyNpc(v.Appearance); });
		}

		private async void ApplyNpc(INpcBase npc)
		{
			AppearanceFile apFile = npc.ToFile();
			await apFile.Apply(this.Actor, AppearanceFile.SaveModes.All);
		}

		private async Task Load()
		{
			FileBase file = await FileService.OpenAny(
				LegacyEquipmentSetFile.FileType,
				LegacyAppearanceFile.AllFileType,
				DatAppearanceFile.FileType,
				AppearanceFile.FileType);

			bool advanced = file?.UseAdvancedLoad ?? false;

			if (file is LegacyAppearanceFile legacyAllFile)
				file = legacyAllFile.Upgrade();

			if (file is LegacyEquipmentSetFile legacyEquipmentFile)
				file = legacyEquipmentFile.Upgrade();

			if (file is DatAppearanceFile datFile)
				file = datFile.Upgrade();

			if (file is AppearanceFile apFile)
			{
				AppearanceFile.SaveModes mode = AppearanceFile.SaveModes.All;

				if (advanced)
				{
					mode = await ViewService.ShowDialog<AppearanceModeSelectorDialog, AppearanceFile.SaveModes>("Load Appearance...");

					if (mode == AppearanceFile.SaveModes.None)
					{
						return;
					}
				}

				await apFile.Apply(this.Actor, mode);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			await FileService.Save(
				async (advancedMode) =>
				{
					AppearanceFile.SaveModes mode = AppearanceFile.SaveModes.All;

					if (advancedMode)
						mode = await ViewService.ShowDialog<AppearanceModeSelectorDialog, AppearanceFile.SaveModes>("Save Appearance...");

					AppearanceFile file = new AppearanceFile();

					if (mode == AppearanceFile.SaveModes.None)
						return null;

					file.Read(this.Actor, mode);

					return file;
				},
				AppearanceFile.FileType);
		}

		private void OnActorChanged(ActorViewModel actor)
		{
			this.Actor = actor;
			bool hasValidSelection = actor != null && (actor.ObjectKind == ActorTypes.Player || actor.ObjectKind == ActorTypes.BattleNpc || actor.ObjectKind == ActorTypes.EventNpc);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = hasValidSelection;
			});
		}

		private void SelectionModeChanged(Modes mode)
		{
			this.IsOverworld = mode == Modes.Overworld;
		}
	}
}
