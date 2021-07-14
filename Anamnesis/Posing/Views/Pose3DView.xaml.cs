// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Views
{
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using PropertyChanged;
	using XivToolsWpf;
	using Colors = System.Windows.Media.Colors;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class Pose3DView : UserControl
	{
		private readonly PerspectiveCamera camera;
		private readonly RotateTransform3D cameraRotaion;
		private readonly TranslateTransform3D cameraPosition;

		public Pose3DView()
		{
			this.InitializeComponent();

			this.camera = new PerspectiveCamera(new Point3D(0, 0.75, -4), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
			this.Viewport.Camera = this.camera;

			this.cameraRotaion = new RotateTransform3D();
			QuaternionRotation3D camRot = new QuaternionRotation3D();
			camRot.Quaternion = CameraService.Instance.Camera?.Rotation3d ?? Quaternion.Identity;
			this.cameraRotaion.Rotation = camRot;
			this.cameraPosition = new TranslateTransform3D();
			Transform3DGroup transformGroup = new Transform3DGroup();
			transformGroup.Children.Add(this.cameraRotaion);
			transformGroup.Children.Add(this.cameraPosition);
			this.camera.Transform = transformGroup;

			this.ContentArea.DataContext = this;

			if (CameraService.Instance.Camera != null)
			{
				CameraService.Instance.Camera.PropertyChanged += this.OnCameraChanged;
			}
		}

		public SkeletonVisual3d? Skeleton { get; set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnDataContextChanged(null, default);
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

		private async void OnCameraChanged(object? sender, PropertyChangedEventArgs? e)
		{
			if (CameraService.Instance == null || CameraService.Instance.Camera == null)
				return;

			await Dispatch.MainThread();

			////this.camera.Position = CameraService.Instance.CameraPosition.ToMedia3DPoint();
			QuaternionRotation3D rot = (QuaternionRotation3D)this.cameraRotaion.Rotation;
			rot.Quaternion = CameraService.Instance.Camera.Rotation3d;
			this.cameraRotaion.Rotation = rot;
		}

		private void OnFrameClicked(object sender, RoutedEventArgs e)
		{
			this.FrameSkeleton();
			this.OnCameraChanged(sender, null);
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

			double d = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ);
			Point3D center = new Point3D(pos.X, pos.Y, pos.Z - (d + 3));
			this.camera.Position = center;

			foreach (BoneVisual3d visual in this.Skeleton.Bones)
			{
				visual.SphereRadius = d / 128;
			}
		}
	}
}
