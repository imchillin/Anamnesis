// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class PosePage : UserControl
	{
		private static PoseFile.Configuration fileConfig = new PoseFile.Configuration();

		public PosePage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public GposeService GposeService => GposeService.Instance;
		public PoseService PoseService { get => PoseService.Instance; }
		public TargetService TargetService { get => TargetService.Instance; }

		public SkeletonVisual3d? Skeleton { get; private set; }

		public PoseFile.Configuration FileConfiguration => fileConfig;

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
				////if (this.Skeleton != null && !this.PoseService.CanEdit)
				////	this.Skeleton.CurrentBone = null;

				this.Skeleton?.ReadTranforms();
			});
		}

		private void OnPoseServiceEnabledChanged(bool value)
		{
			if (!value)
			{
				this.OnClearClicked(null, null);
			}
			else
			{
				Application.Current?.Dispatcher.Invoke(() =>
				{
					this.Skeleton?.ReadTranforms();
				});
			}
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

				if (actor == null || this.Skeleton == null)
					return;

				OpenResult result = await FileService.Open<PoseFile, LegacyPoseFile>("Pose");

				if (result.File == null)
					return;

				if (result.File is LegacyPoseFile legacyFile)
					result.File = legacyFile.Upgrade(actor.Customize?.Race ?? Appearance.Races.Hyur);

				if (result.File is PoseFile poseFile)
				{
					await poseFile.Apply(actor, this.Skeleton, fileConfig);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex, "Pose", Log.Severity.Error);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			ActorViewModel? actor = this.DataContext as ActorViewModel;

			if (actor == null || this.Skeleton == null)
				return;

			PoseFile file = new PoseFile();
			file.WriteToFile(actor, this.Skeleton, fileConfig);
			await FileService.Save(file);
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

		private void OnClearClicked(object? sender, RoutedEventArgs? e)
		{
			if (this.Skeleton != null)
			{
				this.Skeleton.CurrentBone = null;
			}
		}

		private void OnSelectChildrenClicked(object sender, RoutedEventArgs e)
		{
			if (this.Skeleton == null)
				return;

			List<BoneVisual3d> bones = new List<BoneVisual3d>();
			foreach (BoneVisual3d bone in this.Skeleton.SelectedBones)
			{
				bone.GetChildren(ref bones);
			}

			this.Skeleton.Select(bones, SkeletonVisual3d.SelectMode.Add);
		}
	}
}
