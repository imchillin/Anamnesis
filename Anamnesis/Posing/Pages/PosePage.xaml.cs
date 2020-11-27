// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Views;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class PosePage : UserControl
	{
		public const double DragThreshold = 20;

		private static PoseFile.Configuration fileConfig = new PoseFile.Configuration();

		private bool isLeftMouseButtonDownOnWindow;
		private bool isDragging;
		private Point origMouseDownPoint;

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
			this.Skeleton?.ClearSelection();
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

		private void Canvas_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				this.isLeftMouseButtonDownOnWindow = true;
				this.origMouseDownPoint = e.GetPosition(this.SelectionCanvas);
			}
		}

		private void Canvas_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (this.Skeleton == null)
				return;

			Point curMouseDownPoint = e.GetPosition(this.SelectionCanvas);

			if (this.isDragging)
			{
				e.Handled = true;

				this.DragSelectionBorder.Visibility = Visibility.Visible;

				double minx = Math.Min(curMouseDownPoint.X, this.origMouseDownPoint.X);
				double miny = Math.Min(curMouseDownPoint.Y, this.origMouseDownPoint.Y);
				double maxx = Math.Max(curMouseDownPoint.X, this.origMouseDownPoint.X);
				double maxy = Math.Max(curMouseDownPoint.Y, this.origMouseDownPoint.Y);

				minx = Math.Max(minx, 0);
				miny = Math.Max(miny, 0);
				maxx = Math.Min(maxx, this.SelectionCanvas.ActualWidth);
				maxy = Math.Min(maxy, this.SelectionCanvas.ActualHeight);

				Canvas.SetLeft(this.DragSelectionBorder, minx);
				Canvas.SetTop(this.DragSelectionBorder, miny);
				this.DragSelectionBorder.Width = maxx - minx;
				this.DragSelectionBorder.Height = maxy - miny;

				foreach (BoneView bone in BoneView.All)
				{
					if (bone.Bone == null)
						continue;

					Point relativePoint = bone.TransformToAncestor(this.MouseCanvas).Transform(new Point(0, 0));
					if (relativePoint.X > minx && relativePoint.X < maxx && relativePoint.Y > miny && relativePoint.Y < maxy)
					{
						this.Skeleton.Hover(bone.Bone, true);
					}
					else
					{
						this.Skeleton.Hover(bone.Bone, false);
					}
				}
			}
			else if (this.isLeftMouseButtonDownOnWindow)
			{
				System.Windows.Vector dragDelta = curMouseDownPoint - this.origMouseDownPoint;
				double dragDistance = Math.Abs(dragDelta.Length);
				if (dragDistance > DragThreshold)
				{
					this.isDragging = true;
					this.MouseCanvas.CaptureMouse();
					e.Handled = true;
				}
			}
		}

		private void Canvas_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.isLeftMouseButtonDownOnWindow = false;
			if (this.isDragging)
			{
				this.isDragging = false;

				if (this.Skeleton != null)
				{
					double minx = Canvas.GetLeft(this.DragSelectionBorder);
					double miny = Canvas.GetTop(this.DragSelectionBorder);
					double maxx = minx + this.DragSelectionBorder.Width;
					double maxy = miny + this.DragSelectionBorder.Height;

					List<BoneView> toSelect = new List<BoneView>();

					foreach (BoneView bone in BoneView.All)
					{
						if (bone.Bone == null)
							continue;

						if (!bone.IsVisible)
							continue;

						this.Skeleton.Hover(bone.Bone, false);

						Point relativePoint = bone.TransformToAncestor(this.MouseCanvas).Transform(new Point(0, 0));
						if (relativePoint.X > minx && relativePoint.X < maxx && relativePoint.Y > miny && relativePoint.Y < maxy)
						{
							toSelect.Add(bone);
						}
					}

					this.Skeleton.Select(toSelect);
				}

				this.DragSelectionBorder.Visibility = Visibility.Collapsed;
				e.Handled = true;
			}

			this.MouseCanvas.ReleaseMouseCapture();
		}
	}
}
