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
	using System.Windows.Media.Media3D;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Extensions;
	using Anamnesis.PoseModule.Views;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;

	using CmQuaternion = Anamnesis.Memory.Quaternion;
	using Quaternion = System.Windows.Media.Media3D.Quaternion;
	using QuaternionExtensions = Anamnesis.PoseModule.QuaternionExtensions;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class PosePage : UserControl
	{
		public const double DragThreshold = 20;

		private static ILogger Log => Serilog.Log.ForContext<PosePage>();

		private static bool isMirroring = false;

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

		public bool IsMirroring
		{
			get
			{
				return isMirroring;
			}
			set
			{
				isMirroring = value;
			}
		}
		public SkeletonVisual3d? Skeleton { get; private set; }
		public PoseFile.Configuration FileConfiguration => FileConfig;
		
		/* Basic Idea:
		 *	- get mirrored quat of targetBone 
		 *  - check if its a 'left' bone
		 *		- if it is:
		 *			- get the mirrored quat of the corresponding right bone
		 *			- store the first quat (from left bone) on the right bone
		 *			- store the second quat (from right bone) on the left bone
		 *		- if not:
		 *			- store the quat on the target bone
		 *	- recursively mirror on all child bones
		 */
		private void MirrorBone(BoneVisual3d? targetBone, bool shouldFlip = true)
		{
			if (targetBone != null)
			{
				CmQuaternion newRotation = QuaternionExtensions.MirrorQuaternion(targetBone.ViewModel.Rotation); //character-relative transform
				if (shouldFlip && targetBone.BoneName.EndsWith("Left"))
                {
					BoneVisual3d? rightBone = targetBone.Skeleton.GetBone(targetBone.BoneName.Replace("Left", "Right"));
					if(rightBone != null)
                    {
						CmQuaternion rightRot = QuaternionExtensions.MirrorQuaternion(rightBone.ViewModel.Rotation);
						targetBone.ViewModel.Rotation = rightRot;
						rightBone.ViewModel.Rotation = newRotation;
                    }
					else
                    {
						Log.Debug("could not find right bone of: " + targetBone.BoneName);
                    }
                }
				else if (shouldFlip && targetBone.BoneName.EndsWith("Right"))
                {
					//do nothing so it doesn't revert...
                }
				else
                {
					targetBone.ViewModel.Rotation = newRotation;
                }

				if (PoseService.Instance.EnableParenting)
                {
					foreach (Visual3D? child in targetBone.Children)
					{
						if (child is BoneVisual3d childBone)
						{
							MirrorBone(childBone, shouldFlip);
						}
					}
                }
			}
		}

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

				this.Skeleton?.Reselect();
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

			try
			{
				this.Skeleton = await PoseService.GetVisual(actor);

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
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to bind skeleton to view");
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
			await PoseFile.Save(actor, this.Skeleton, FileConfig);
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

		private void OnMirrorClicked(object sender, System.Windows.RoutedEventArgs e)
		{
			if (this.Skeleton != null && !this.IsMirroring)
			{// if no bone selected, mirror both lumbar and waist bones
				this.IsMirroring = true;
				if (this.Skeleton.CurrentBone == null)
				{
					BoneVisual3d? waistBone = this.Skeleton.GetBone("Waist");
					BoneVisual3d? lumbarBone = this.Skeleton.GetBone("SpineA");
					MirrorBone(waistBone);
					MirrorBone(lumbarBone);
					waistBone?.ReadTransform(true);
					lumbarBone?.ReadTransform(true);
				}
				else
				{// if targeted bone is a limb don't switch the respective left and right sides
					BoneVisual3d targetBone = this.Skeleton.CurrentBone;
					if (targetBone.BoneName.EndsWith("Left") || targetBone.BoneName.EndsWith("Right"))
                    {
						MirrorBone(targetBone, false);
                    }
					else
                    {
						MirrorBone(targetBone);
                    }
					targetBone.ReadTransform(true);
				}
				this.IsMirroring = false;
			}
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
			if (!this.isLeftMouseButtonDownOnWindow)
				return;

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
