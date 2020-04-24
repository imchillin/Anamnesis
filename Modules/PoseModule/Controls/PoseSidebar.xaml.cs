// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Controls
{
	using System;
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for PoseSidebar.xaml.
	/// </summary>
	public partial class PoseSidebar : UserControl
	{
		public PoseSidebar()
		{
			this.InitializeComponent();
		}

		private async void OnOpenClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				IFileService fileService = Services.Get<IFileService>();
				FileBase file = await fileService.OpenAny(PoseFile.FileType, LegacyPoseFile.FileType);

				if (file is LegacyPoseFile legacyFile)
					file = legacyFile.Upgrade();

				Module.SkeletonViewModel.IsEnabled = true;

				if (file is PoseFile poseFile)
				{
					poseFile.Write(Module.SkeletonViewModel.Bones);
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
			file.Read(Module.SkeletonViewModel.Bones);

			await fileService.SaveAs(file);
		}
	}
}
