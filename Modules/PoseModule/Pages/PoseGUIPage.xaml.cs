// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	public partial class PoseGuiPage : UserControl
	{
		public PoseGuiPage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = Module.SkeletonViewModel;
		}

		/*private async void OnOpenClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				IFileService fileService = Services.Get<IFileService>();
				FileBase file = await fileService.OpenAny(PoseFile.FileType, LegacyPoseFile.FileType);

				if (file is LegacyPoseFile legacyFile)
					file = legacyFile.Upgrade();

				this.ViewModel.IsEnabled = true;

				if (file is PoseFile poseFile)
				{
					poseFile.Write(this.ViewModel.Bones);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex, "Pose", Log.Severity.Error);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			IFileService fileService = Services.Get<IFileService>();

			PoseFile file = new PoseFile();
			file.Read(this.ViewModel.Bones);

			await fileService.SaveAs(file);
		}*/
	}
}
