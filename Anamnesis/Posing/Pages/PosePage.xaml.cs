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
	public partial class PosePage : UserControl
	{
		private static PoseFile.Configuration? lastAdvancedLoadConfig;
		private static PoseFile.Configuration? lastAdvancedSaveConfig;

		public PosePage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public GposeService GposeService => GposeService.Instance;
		public PoseService PoseService { get => PoseService.Instance; }
		public TargetService TargetService { get => TargetService.Instance; }

		public SkeletonVisual3d? Skeleton { get; private set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnDataContextChanged(null, default);

			PoseService.EnabledChanged += this.OnPoseServiceEnabledChanged;
			this.PoseService.PropertyChanged += this.PoseService_PropertyChanged;
		}

		private void PoseService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Application.Current?.Dispatcher.Invoke(() =>
			{
				if (this.Skeleton != null && !this.PoseService.CanEdit)
					this.Skeleton.CurrentBone = null;

				this.Skeleton?.ReadTranforms();
			});
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
			if (!this.IsVisible)
				return;

			ActorViewModel? actor = this.DataContext as ActorViewModel;

			if (actor == null)
			{
				this.Skeleton = null;
				return;
			}

			this.Skeleton?.Clear();

			this.Skeleton = new SkeletonVisual3d(actor);

			////this.ThreeDView.DataContext = this.Skeleton;
			this.GuiView.DataContext = this.Skeleton;
			this.MatrixView.DataContext = this.Skeleton;
		}

		private async void OnOpenClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				ActorViewModel? actor = this.DataContext as ActorViewModel;

				if (actor == null)
					return;

				OpenResult result = await FileService.Open<PoseFile, LegacyPoseFile>("Pose");

				if (result.File == null)
					return;

				if (result.File is LegacyPoseFile legacyFile)
					result.File = legacyFile.Upgrade(actor.Customize?.Race ?? Appearance.Races.Hyur);

				PoseFile.Configuration? config = new PoseFile.Configuration();

				if (result.UseAdvancedLoad)
				{
					config = await ViewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Configuration?>("Load Pose...", lastAdvancedLoadConfig);

					if (config == null)
						return;

					lastAdvancedLoadConfig = config;
				}

				if (result.File is PoseFile poseFile)
				{
					await poseFile.Apply(actor, config);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex, "Pose", Log.Severity.Error);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			await FileService.Save(this.Save, "Pose");
		}

		private async Task<PoseFile?> Save(bool useAdvancedSave)
		{
			ActorViewModel? actor = this.DataContext as ActorViewModel;

			if (actor == null)
				return null;

			PoseFile.Configuration? config = new PoseFile.Configuration();

			if (useAdvancedSave)
			{
				config = await ViewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Configuration?>("Save Pose...", lastAdvancedSaveConfig);

				if (config == null)
					return null;

				lastAdvancedSaveConfig = config;
			}

			PoseFile file = new PoseFile();
			file.WriteToFile(actor, config);

			return file;
		}

		private void OnViewChanged(object sender, SelectionChangedEventArgs e)
		{
			int selected = this.ViewSelector.SelectedIndex;

			if (this.GuiView == null)
				return;

			this.GuiView.Visibility = selected == 0 ? Visibility.Visible : Visibility.Collapsed;
			this.MatrixView.Visibility = selected == 1 ? Visibility.Visible : Visibility.Collapsed;
			////this.ThreeDView.Visibility = selected == 2 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void OnEditCharacterChecked(object sender, RoutedEventArgs e)
		{
			if (this.Skeleton != null)
			{
				this.Skeleton.CurrentBone = null;
			}
		}
	}
}
