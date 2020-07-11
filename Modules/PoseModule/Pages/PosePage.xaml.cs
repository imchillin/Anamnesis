// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Pages
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.PoseModule.Dialogs;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class PosePage : UserControl
	{
		public PosePage()
		{
			this.PoseService = Services.Get<PoseService>();
			this.PoseService.EnabledChanged += this.OnEnabledChanged;
			this.PoseService.AvailableChanged += this.OnAvailableChanged;
			this.PoseService.FreezePhysicsChanged += this.OnFreezePhysicsChanged;

			this.InitializeComponent();

			this.SkeletonViewModel = new SkeletonViewModel();
			this.ContentArea.DataContext = this;

			this.OnEnabledChanged(this.PoseService.IsEnabled);
			this.OnAvailableChanged(this.PoseService.IsAvailable);
		}

		public PoseService PoseService { get; private set; }
		public SkeletonViewModel SkeletonViewModel { get; set; }

		public bool CanPose { get; set; }

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
			await this.SkeletonViewModel.Initialize(this.DataContext as Actor);
		}

		[SuppressPropertyChangedWarnings]
		private void OnAvailableChanged(bool value)
		{
			this.CanPose = value;
		}

		[SuppressPropertyChangedWarnings]
		private void OnEnabledChanged(bool value)
		{
			if (this.SkeletonViewModel != null)
			{
				this.SkeletonViewModel.CurrentBone = null;
			}
		}

		[SuppressPropertyChangedWarnings]
		private void OnFreezePhysicsChanged(bool value)
		{
			if (this.SkeletonViewModel != null)
			{
				this.SkeletonViewModel.CurrentBone = null;
			}
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

				this.SkeletonViewModel.CurrentBone = null;

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

		[SuppressPropertyChangedWarnings]
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
