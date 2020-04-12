// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Files;
	using ConceptMatrix.Services;

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
			IFileService fileService = Module.Services.Get<IFileService>();
			FileBase file = await fileService.OpenAny(EquipmentSetFile.FileType, LegacyEquipmentSetFile.FileType);

			if (file is LegacyEquipmentSetFile legacyFile)
				file = legacyFile.Upgrade();

			if (file is EquipmentSetFile eqFile)
			{
				this.Equipment.Load(eqFile);
			}
		}

		private void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			IFileService fileService = Module.Services.Get<IFileService>();
			EquipmentSetFile eqFile = this.Equipment.Save();
			fileService.Save(eqFile);
		}
	}
}
