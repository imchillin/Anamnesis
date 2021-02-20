// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Pages
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Views;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class PosePage : UserControl
	{
		public const double DragThreshold = 20;

		public static PoseFile.Configuration FileConfig = new PoseFile.Configuration();

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
		public PoseFile.Configuration FileConfiguration => FileConfig;

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

		private async void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
		{
			ActorViewModel? actor = this.DataContext as ActorViewModel;

			if (actor == null)
			{
				this.Skeleton = null;
				return;
			}

			if (!this.IsVisible)
				return;

			if (this.Skeleton != null)
			{
				if (this.Skeleton.Actor != actor)
				{
					this.Skeleton.Clear();
					this.Skeleton = new SkeletonVisual3d(actor);
					await this.Skeleton.GenerateBones();
				}
			}
			else
			{
				this.Skeleton = new SkeletonVisual3d(actor);
				await this.Skeleton.GenerateBones();
			}

			this.ThreeDView.DataContext = this.Skeleton;
			this.GuiView.DataContext = this.Skeleton;
			this.MatrixView.DataContext = this.Skeleton;

			if (this.Skeleton.File != null)
			{
				if (!this.Skeleton.File.AllowPoseGui)
				{
					this.ViewSelector.SelectedIndex = 1;
				}

				if (!this.Skeleton.File.AllowPoseMatrix)
				{
					this.ViewSelector.SelectedIndex = 2;
				}
			}
		}

		private async void OnOpenClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				ActorViewModel? actor = this.DataContext as ActorViewModel;

				if (actor == null || this.Skeleton == null)
					return;

				OpenResult result = await FileService.Open<PoseFile, LegacyPoseFile>();

				if (result.File == null)
					return;

				if (result.File is LegacyPoseFile legacyFile)
					result.File = legacyFile.Upgrade(actor.Customize?.Race ?? Appearance.Races.Hyur);

				if (result.File is PoseFile poseFile)
				{
					await poseFile.Apply(actor, this.Skeleton, FileConfig);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to load pose file");
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			ActorViewModel? actor = this.DataContext as ActorViewModel;

			if (actor == null || this.Skeleton == null)
				return;

			SaveResult result = await FileService.Save<PoseFile>();

			if (string.IsNullOrEmpty(result.Path) || result.Info == null)
				return;

			PoseFile file = new PoseFile();
			file.WriteToFile(actor, this.Skeleton, FileConfig);

			using FileStream stream = new FileStream(result.Path, FileMode.Create);
			result.Info.SerializeFile(file, stream);
		}

		private void OnViewChanged(object sender, SelectionChangedEventArgs e)
		{
			int selected = this.ViewSelector.SelectedIndex;

			if (this.GuiView == null)
				return;

			this.GuiView.Visibility = selected == 0 ? Visibility.Visible : Visibility.Collapsed;
			this.MatrixView.Visibility = selected == 1 ? Visibility.Visible : Visibility.Collapsed;
			this.ThreeDView.Visibility = selected == 2 ? Visibility.Visible : Visibility.Collapsed;
			this.FlipSidesOption.Visibility = this.GuiView.Visibility;
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

		private void OnCanvasMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				this.isLeftMouseButtonDownOnWindow = true;
				this.origMouseDownPoint = e.GetPosition(this.SelectionCanvas);
			}
		}

		private void OnCanvasMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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

					if (!bone.IsDescendantOf(this.MouseCanvas))
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

		private void OnCanvasMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
			else
			{
				if (this.Skeleton != null && !this.Skeleton.HasHover)
				{
					this.Skeleton?.ClearSelection();
				}
			}

			this.MouseCanvas.ReleaseMouseCapture();
		}
	}
}
