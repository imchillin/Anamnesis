// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Controls
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.PoseModule.Dialogs;

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

				if (file == null)
					return;

				if (file is LegacyPoseFile legacyFile)
					file = legacyFile.Upgrade(Module.SkeletonViewModel.Race);

				IViewService viewService = Services.Get<IViewService>();
				PoseFile.Groups groups = await viewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Groups>("Load Pose...");

				if (groups == PoseFile.Groups.None)
					return;

				Module.SkeletonViewModel.IsEnabled = true;

				if (file is PoseFile poseFile)
				{
					poseFile.Write(Module.SkeletonViewModel.Bones, groups);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex, "Pose", Log.Severity.Error);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();
			PoseFile.Groups groups = await viewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Groups>("Save Pose...");

			if (groups == PoseFile.Groups.None)
				return;

			IFileService fileService = Services.Get<IFileService>();
			PoseFile file = new PoseFile();
			file.Read(Module.SkeletonViewModel.Bones, groups);

			await fileService.SaveAs(file);
		}
	}
}
