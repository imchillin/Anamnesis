// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Files;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	public partial class AppearancePage : UserControl
	{
		private IBaseMemoryOffset baseOffset;

		public AppearancePage()
		{
			this.InitializeComponent();

			ISelectionService selectionService = Services.Get<ISelectionService>();
			selectionService.SelectionChanged += this.OnSelectionChanged;
			this.OnSelectionChanged(selectionService.CurrentSelection);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			ISelectionService selectionService = Services.Get<ISelectionService>();
			selectionService.SelectionChanged -= this.OnSelectionChanged;
		}

		private async void OnOpenClicked(object sender, RoutedEventArgs e)
		{
			IActorRefreshService refreshService = Services.Get<IActorRefreshService>();
			IFileService fileService = Services.Get<IFileService>();

			FileBase file = await fileService.OpenAny(
				EquipmentSetFile.FileType,
				LegacyEquipmentSetFile.FileType,
				AppearanceSetFile.FileType,
				AllFile.FileType,
				LegacyAllFile.AllFileType);

			if (file is LegacyAllFile legacyAllFile)
				file = legacyAllFile.Upgrade();

			if (file is LegacyEquipmentSetFile legacyEquipmentFile)
				file = legacyEquipmentFile.Upgrade();

			if (file is EquipmentSetFile eqFile)
			{
				eqFile.WritePreRefresh(this.Equipment);
				await refreshService.RefreshAsync(this.baseOffset);
				eqFile.WritePostRefresh(this.Equipment);
			}
			else if (file is AppearanceSetFile apFile)
			{
				if (this.Appearance.ViewModel != null)
				{
					apFile.Write(this.Appearance.ViewModel);
				}
			}
			else if (file is AllFile allFile)
			{
				if (this.Appearance.ViewModel != null)
					allFile.Appearance.Write(this.Appearance.ViewModel);

				allFile.Equipment.WritePreRefresh(this.Equipment);
				await refreshService.RefreshAsync(this.baseOffset);
				allFile.Equipment.WritePostRefresh(this.Equipment);
			}
		}

		private async void OnSaveEquipmentClicked(object sender, RoutedEventArgs e)
		{
			IFileService fileService = Services.Get<IFileService>();
			EquipmentSetFile file = new EquipmentSetFile();
			file.Read(this.Equipment);
			await fileService.Save(file);
		}

		private async void OnSaveAppearanceClicked(object sender, RoutedEventArgs e)
		{
			IFileService fileService = Services.Get<IFileService>();
			AppearanceSetFile file = new AppearanceSetFile();
			file.Read(this.Appearance.ViewModel);
			await fileService.Save(file);
		}

		private async void OnSaveAllClicked(object sender, RoutedEventArgs e)
		{
			IFileService fileService = Services.Get<IFileService>();
			AllFile file = new AllFile();
			file.Appearance.Read(this.Appearance.ViewModel);
			file.Equipment.Read(this.Equipment);
			await fileService.Save(file);
		}

		private void OnSelectionChanged(Selection selection)
		{
			bool hasValidSelection = selection != null && (selection.Type == ActorTypes.Player || selection.Type == ActorTypes.BattleNpc || selection.Type == ActorTypes.EventNpc);

			if (hasValidSelection)
				this.baseOffset = selection.BaseAddress;

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = hasValidSelection;
			});
		}
	}
}
