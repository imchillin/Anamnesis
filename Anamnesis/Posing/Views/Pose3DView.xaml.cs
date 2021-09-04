// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Views
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media.Media3D;
	using Anamnesis.Posing.Visuals;
	using PropertyChanged;
	using XivToolsWpf;
	using Colors = System.Windows.Media.Colors;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class Pose3DView : UserControl
	{
		public readonly PerspectiveCamera Camera;
		public readonly RotateTransform3D CameraRotaion;
		public readonly TranslateTransform3D CameraPosition;

		private bool cameraIsTicking = false;

		public Pose3DView()
		{
			this.InitializeComponent();

			this.Camera = new PerspectiveCamera(new Point3D(0, 0.75, -4), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
			this.Viewport.Camera = this.Camera;

			this.CameraRotaion = new RotateTransform3D();
			QuaternionRotation3D camRot = new QuaternionRotation3D();
			camRot.Quaternion = CameraService.Instance.Camera?.Rotation3d ?? Quaternion.Identity;
			this.CameraRotaion.Rotation = camRot;
			this.CameraPosition = new TranslateTransform3D();
			Transform3DGroup transformGroup = new Transform3DGroup();
			transformGroup.Children.Add(this.CameraRotaion);
			transformGroup.Children.Add(this.CameraPosition);
			this.Camera.Transform = transformGroup;

			this.ContentArea.DataContext = this;

			if (CameraService.Instance.Camera != null)
			{
				CameraService.Instance.Camera.PropertyChanged += this.OnCameraChanged;
			}

			Task.Run(this.UpdateCamera);
		}

		public SkeletonVisual3d? Skeleton { get; set; }

		public double CameraDistance { get; set; }
		public Quaternion CameraRotation { get; set; }

		public Quaternion Billboard { get; private set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnDataContextChanged(null, default);
			Task.Run(this.UpdateCamera);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.SkeletonRoot.Children.Clear();
		}

		private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
		{
			this.Skeleton = this.DataContext as SkeletonVisual3d;

			if (this.Skeleton == null)
				return;

			this.Skeleton.PropertyChanged += this.OnSkeletonPropertyChanged;
			this.SkeletonRoot.Children.Clear();

			if (!this.SkeletonRoot.Children.Contains(this.Skeleton))
				this.SkeletonRoot.Children.Add(this.Skeleton);

			this.SkeletonRoot.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });

			this.FrameSkeleton();
		}

		private void OnSkeletonPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SkeletonVisual3d.Generating))
			{
				this.FrameSkeleton();
			}
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SkeletonVisual3d? vm = this.DataContext as SkeletonVisual3d;

			if (vm == null)
				return;

			if (e.AddedItems == null || e.AddedItems.Count <= 0)
				return;

			BoneVisual3d? selected = e.AddedItems[0] as BoneVisual3d;

			if (selected == null)
				return;

			vm.Hover(selected, true);
			vm.Select(selected);
		}

		private void OnCameraChanged(object? sender, PropertyChangedEventArgs? e)
		{
			if (CameraService.Instance == null || CameraService.Instance.Camera == null)
				return;
		}

		private void OnFrameClicked(object sender, RoutedEventArgs e)
		{
			this.FrameSkeleton();
		}

		private void OnRegenerateSkeletonClicked(object sender, RoutedEventArgs e)
		{
			if (this.Skeleton?.File != null && this.Skeleton.File.IsGeneratedParenting)
			{
				Task.Run(async () =>
				{
					await Dispatch.MainThread();
					await this.Skeleton.GenerateBones(true);
				});
			}
		}

		private void FrameSkeleton()
		{
			// position camera at average center position of skeleton
			if (this.Skeleton == null || this.Skeleton.Bones == null || this.Skeleton.Bones.Count <= 0)
				return;

			Rect3D bounds = default;

			Vector3D pos = this.Skeleton.Bones.First().ViewModel.Position.ToMedia3DVector();
			foreach (BoneVisual3d visual in this.Skeleton.Bones)
			{
				pos += visual.ViewModel.Position.ToMedia3DVector();

				Point3D point = visual.ViewModel.Position.ToMedia3DPoint();
				bounds.Union(point);
			}

			pos /= this.Skeleton.Bones.Count;

			this.CameraDistance = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ);
		}

		private void Viewport_MouseMove(object sender, MouseEventArgs e)
		{
		}

		private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			this.CameraDistance -= e.Delta / 120;
			this.CameraDistance = Math.Clamp(this.CameraDistance, 0, 300);
		}

		private async Task UpdateCamera()
		{
			if (this.cameraIsTicking)
				return;

			this.cameraIsTicking = true;

			await Dispatch.MainThread();

			while (this.IsLoaded)
			{
				if (!this.IsVisible)
					await Task.Delay(100);

				await Task.Delay(33);
				await Dispatch.MainThread();

				if (this.Skeleton == null || CameraService.Instance.Camera == null)
					continue;

				// TODO: allow the user to rotate camera with the mouse instead
				this.CameraRotation = CameraService.Instance.Camera.Rotation3d;

				// Update the billboard rotation
				Vector3D angle = this.CameraRotation.ToEulerAngles();
				angle.Y -= 45;
				angle.Y += 180;
				angle.Z = -angle.Z;
				this.Billboard = angle.ToQuaternion();

				// Apply camera rotation
				QuaternionRotation3D rot = (QuaternionRotation3D)this.CameraRotaion.Rotation;
				rot.Quaternion = this.CameraRotation;
				this.CameraRotaion.Rotation = rot;

				// Apply camera position
				Point3D pos = this.Camera.Position;
				pos.Z = -this.CameraDistance;
				this.Camera.Position = pos;

				// Notify bone targets that the camera has updated
				foreach (BoneTargetVisual3d visual in this.Skeleton.BoneTargets)
				{
					visual.OnCameraUpdated(this);
				}
			}

			this.cameraIsTicking = false;
		}
	}
}
