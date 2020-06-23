// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using ConceptMatrix.AppearanceModule.Dialogs;
	using ConceptMatrix.AppearanceModule.Files;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	public partial class AppearancePage : UserControl
	{
		private ISelectionService selectionService;
		private IActorRefreshService refreshService;
		private Actor actor;

		private AppearanceFile refreshFile;

		public AppearancePage()
		{
			this.selectionService = Services.Get<ISelectionService>();
			this.refreshService = Services.Get<IActorRefreshService>();

			this.refreshService.RefreshBegin += this.RefreshService_RefreshBegin;
			this.refreshService.RefreshComplete += this.RefreshService_RefreshComplete;

			this.InitializeComponent();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.selectionService.ModeChanged += this.SelectionModeChanged;
			this.OnActorChanged(this.DataContext as Actor);
			this.SelectionModeChanged(this.selectionService.GetMode());
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.selectionService.ModeChanged -= this.SelectionModeChanged;
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
				await this.refreshService.RefreshAsync(this.actor);

				////apFile.Write(this.Appearance, this.Equipment, mode);
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
			this.actor = actor;
			bool hasValidSelection = actor != null && (actor.Type == ActorTypes.Player || actor.Type == ActorTypes.BattleNpc || actor.Type == ActorTypes.EventNpc);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = hasValidSelection;
			});
		}

		private async void SelectionModeChanged(Modes mode)
		{
			await Task.Delay(1);
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.Equipment.IsEnabled = mode == Modes.Overworld;
			});
		}

		private void RefreshService_RefreshComplete(Actor actor)
		{
			if (this.refreshFile != null)
			{
				this.refreshFile.Write(this.Appearance, this.Equipment, AppearanceFile.SaveModes.All);
				this.refreshFile = null;
			}

			this.IsEnabled = true;
		}

		private void RefreshService_RefreshBegin(Actor actor)
		{
			this.IsEnabled = false;
			this.refreshFile = new AppearanceFile();
			this.refreshFile.Read(this.Appearance, this.Equipment, AppearanceFile.SaveModes.All);
		}
	}
}
