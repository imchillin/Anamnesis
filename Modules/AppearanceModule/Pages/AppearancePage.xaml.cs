// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Dialogs;
	using ConceptMatrix.AppearanceModule.Files;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	public partial class AppearancePage : UserControl
	{
		public AppearancePage()
		{
			this.InitializeComponent();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as Actor);
		}

		private async void OnLoadClicked(object sender, RoutedEventArgs e)
		{
			IFileService fileService = Services.Get<IFileService>();
			IViewService viewService = Services.Get<IViewService>();

			FileBase file = await fileService.OpenAny(
				LegacyEquipmentSetFile.FileType,
				LegacyAppearanceFile.AllFileType,
				AppearanceFile.FileType);

			if (file is LegacyAppearanceFile legacyAllFile)
				file = legacyAllFile.Upgrade();

			if (file is LegacyEquipmentSetFile legacyEquipmentFile)
				file = legacyEquipmentFile.Upgrade();

			if (file is AppearanceFile apFile)
			{
				AppearanceFile.SaveModes mode = await viewService.ShowDialog<AppearanceModeSelectorDialog, AppearanceFile.SaveModes>("Load Appearance...");

				if (mode == AppearanceFile.SaveModes.None)
					return;

				apFile.Write(this.Appearance, this.Equipment, mode);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();
			AppearanceFile.SaveModes mode = await viewService.ShowDialog<AppearanceModeSelectorDialog, AppearanceFile.SaveModes>("Save Appearance...");

			if (mode == AppearanceFile.SaveModes.None)
				return;

			IFileService fileService = Services.Get<IFileService>();
			AppearanceFile file = new AppearanceFile();
			file.Read(this.Appearance, this.Equipment, mode);
			await fileService.Save(file);
		}

		private void OnActorChanged(Actor actor)
		{
			bool hasValidSelection = actor != null && (actor.Type == ActorTypes.Player || actor.Type == ActorTypes.BattleNpc || actor.Type == ActorTypes.EventNpc);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = hasValidSelection;
			});
		}
	}
}
