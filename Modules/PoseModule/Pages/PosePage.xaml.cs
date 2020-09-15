// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Pages
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Dialogs;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class PosePage : UserControl
	{
		public PosePage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public PoseService PoseService { get => PoseModule.PoseService.Instance; }

		public SkeletonVisual3d Skeleton { get; private set; }
		public bool CanPose { get; set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.CanPose = true;
			this.OnDataContextChanged(null, default);

			DateTime dt = DateTime.Now;
			if (dt.Month == 10 && dt.Day == 31)
			{
				this.View3dButton.ToolTip = "Spoopy Skeletons";
			}

			PoseService.EnabledChanged += this.OnPoseServiceEnabledChanged;
		}

		private void OnPoseServiceEnabledChanged(bool value)
		{
			Application.Current?.Dispatcher.Invoke(() =>
			{
				this.Skeleton?.ReadTranforms();
			});
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			ActorViewModel actor = this.DataContext as ActorViewModel;

			if (actor == null)
			{
				this.Skeleton = null;
				return;
			}

			this.Skeleton = new SkeletonVisual3d(actor);

			this.ThreeDView.DataContext = this.Skeleton;
			this.GuiView.DataContext = this.Skeleton;
			this.MatrixView.DataContext = this.Skeleton;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.Skeleton = null;
		}

		private void OnOpenClicked(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
			/*try
			{
				FileBase file = await FileService.OpenAny(PoseFile.FileType, LegacyPoseFile.FileType);

				if (file == null)
					return;

				if (file is LegacyPoseFile legacyFile)
					file = legacyFile.Upgrade(this.SkeletonViewModel.Race);

				PoseFile.Configuration config = new PoseFile.Configuration();

				if (file.UseAdvancedLoad)
					config = await ViewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Configuration>("Load Pose...");

				if (config == null)
					return;

				if (file is PoseFile poseFile)
				{
					await poseFile.Write(this.SkeletonViewModel, config);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex, "Pose", Log.Severity.Error);
			}*/
		}

		private void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			/*IViewService viewService = Services.Get<IViewService>();
			IFileService fileService = Services.Get<IFileService>();

			await fileService.Save(
				async (advancedMode) =>
				{
					PoseFile.Configuration config;

					if (advancedMode)
					{
						config = await viewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Configuration>("Save Pose...");
					}
					else
					{
						config = new PoseFile.Configuration();
					}

					if (config == null)
						return null;

					PoseFile file = new PoseFile();
					file.Read(this.SkeletonViewModel.Bones, config);

					return file;
				},
				PoseFile.FileType);*/

			throw new NotImplementedException();
		}

		[SuppressPropertyChangedWarnings]
		private void OnViewChanged(object sender, SelectionChangedEventArgs e)
		{
			int selected = this.ViewSelector.SelectedIndex;

			if (this.GuiView == null)
				return;

			this.GuiView.Visibility = selected == 0 ? Visibility.Visible : Visibility.Collapsed;
			this.MatrixView.Visibility = selected == 1 ? Visibility.Visible : Visibility.Collapsed;
			this.ThreeDView.Visibility = selected == 2 ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
