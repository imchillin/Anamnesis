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
		public AppearancePage()
		{
			this.InitializeComponent();
		}

		private async void OnOpenClicked(object sender, RoutedEventArgs e)
		{
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
				eqFile.Write(this.Equipment);
			}
			else if (file is AppearanceSetFile apFile)
			{
				apFile.Write(this.Appearance.ViewModel);
			}
			else if (file is AllFile allFile)
			{
				allFile.Appearance.Write(this.Appearance.ViewModel);
				allFile.Equipment.Write(this.Equipment);
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
	}
}
