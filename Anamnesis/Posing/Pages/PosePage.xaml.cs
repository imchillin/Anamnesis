// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Pages
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Dialogs;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class PosePage : UserControl
	{
		public PosePage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public GposeService GposeService => GposeService.Instance;
		public PoseService PoseService { get => PoseModule.PoseService.Instance; }

		public SkeletonVisual3d? Skeleton { get; private set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
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

		private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
		{
			ActorViewModel? actor = this.DataContext as ActorViewModel;

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

		private async void OnOpenClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				ActorViewModel? actor = this.DataContext as ActorViewModel;

				if (actor == null)
					return;

				FileBase? file = await FileService.Open<PoseFile, LegacyPoseFile>();

				if (file == null)
					return;

				if (file is LegacyPoseFile legacyFile)
					file = legacyFile.Upgrade(actor.Customize?.Race ?? Appearance.Races.Hyur);

				PoseFile.Configuration config = new PoseFile.Configuration();

				if (true)
					config = await ViewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Configuration>("Load Pose...");

				if (config == null)
					return;

				if (file is PoseFile poseFile)
				{
					await poseFile.ReadFromFile(actor, config);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex, "Pose", Log.Severity.Error);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			await FileService.Save(
				async (advancedMode) =>
				{
					ActorViewModel? actor = this.DataContext as ActorViewModel;

					if (actor == null)
						return null;

					PoseFile.Configuration config;

					if (advancedMode)
					{
						config = await ViewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Configuration>("Save Pose...");
					}
					else
					{
						config = new PoseFile.Configuration();
					}

					if (config == null)
						return null;

					PoseFile file = new PoseFile();
					file.WriteToFile(actor, config);

					return file;
				});
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
