// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Views
{
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	public partial class Pose3DView : UserControl
	{
		private PerspectiveCamera camera;

		public Pose3DView()
		{
			this.InitializeComponent();

			this.camera = new PerspectiveCamera(new Point3D(0, 0.75, -4), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
			this.Viewport.Camera = this.camera;

			this.Viewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });

			////Anamnesis.Quaternion rootrot = Module.SkeletonViewModel.GetBone("Root").RootRotation;
			////this.root.Transform = new RotateTransform3D(new QuaternionRotation3D(new Quaternion(rootrot.X, rootrot.Y, rootrot.Z, rootrot.W)));
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnDataContextChanged(null, default);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			SkeletonVisual3d? vm = this.DataContext as SkeletonVisual3d;

			if (vm == null)
				return;

			this.Viewport.Children.Remove(vm);
		}

		private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
		{
			SkeletonVisual3d? vm = this.DataContext as SkeletonVisual3d;

			if (vm == null)
				return;

			this.Viewport.Children.Clear();

			if (!this.Viewport.Children.Contains(vm))
				this.Viewport.Children.Add(vm);

			// position camera at average center position of skeleton
			if (vm.Bones != null && vm.Bones.Count > 0)
			{
				Vector3D pos = vm.Bones.First().Value.ViewModel.Position.ToMedia3DVector();
				foreach (BoneVisual3d visual in vm.Bones.Values)
				{
					pos += visual.ViewModel.Position.ToMedia3DVector();
				}

				pos /= vm.Bones.Count;
				Point3D center = new Point3D(pos.X, pos.Y, pos.Z - 4);
				this.camera.Position = center;
			}
		}
	}
}
