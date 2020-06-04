// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.PoseModule.Dialogs;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	public partial class PosePage : UserControl
	{
		private PoseService poseService;

		public PosePage()
		{
			this.poseService = Services.Get<PoseService>();

			this.InitializeComponent();

			this.SkeletonViewModel = new SkeletonViewModel();
			this.ContentArea.DataContext = this.SkeletonViewModel;

			this.TopBarArea.DataContext = this;
		}

		public SkeletonViewModel SkeletonViewModel { get; set; }

		public bool PosingEnabled
		{
			get => this.poseService.IsEnabled;
			set => this.poseService.IsEnabled = value;
		}

		public bool FreezePhysics
		{
			get => this.poseService.FreezePhysics;
			set => this.poseService.FreezePhysics = value;
		}

		public bool FlipSides
		{
			get => this.SkeletonViewModel.FlipSides;
			set => this.SkeletonViewModel.FlipSides = value;
		}

		public bool ParentingEnabled
		{
			get => this.SkeletonViewModel.ParentingEnabled;
			set => this.SkeletonViewModel.ParentingEnabled = value;
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.SkeletonViewModel.Clear();
			await this.SkeletonViewModel.Initialize(this.DataContext as Actor);
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
					file = legacyFile.Upgrade(this.SkeletonViewModel.Race);

				IViewService viewService = Services.Get<IViewService>();
				PoseFile.Groups groups = await viewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Groups>("Load Pose...");

				if (groups == PoseFile.Groups.None)
					return;

				if (file is PoseFile poseFile)
				{
					await poseFile.Write(this.SkeletonViewModel, groups);
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
			file.Read(this.SkeletonViewModel.Bones, groups);

			await fileService.SaveAs(file);
		}

		private void OnViewChanged(object sender, SelectionChangedEventArgs e)
		{
			int selected = this.ViewSelector.SelectedIndex;

			if (this.GuiView == null)
				return;

			this.GuiView.Visibility = selected == 0 ? Visibility.Visible : Visibility.Hidden;
			this.ThreeDView.Visibility = selected == 1 ? Visibility.Visible : Visibility.Hidden;
			this.MatrixView.Visibility = selected == 2 ? Visibility.Visible : Visibility.Hidden;
		}
	}
}
