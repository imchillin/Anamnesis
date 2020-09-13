// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using Anamnesis.Memory;
	using Anamnesis.ThreeD;
	using PropertyChanged;

	using Quaternion = System.Windows.Media.Media3D.Quaternion;

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
			SkeletonVisual3d vm = this.DataContext as SkeletonVisual3d;

			if (vm == null)
				return;

			foreach (BoneVisual3d visual in vm.RootBones)
			{
				this.Viewport.Children.Remove(visual);
			}
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			SkeletonVisual3d vm = this.DataContext as SkeletonVisual3d;

			if (vm == null)
				return;

			this.Viewport.Children.Clear();

			foreach (BoneVisual3d visual in vm.RootBones)
			{
				if (this.Viewport.Children.Contains(visual))
					return;

				this.Viewport.Children.Add(visual);
			}

			// position camera at average center position of skeleton
			if (vm.Bones.Count > 0)
			{
				Vector3D pos = vm.Bones[0].LiveTransform.Position.ToMedia3DVector();
				foreach (BoneVisual3d visual in vm.Bones)
				{
					pos += visual.LiveTransform.Position.ToMedia3DVector();
				}

				pos /= vm.Bones.Count;
				Point3D center = new Point3D(pos.X, pos.Y, pos.Z - 4);
				this.camera.Position = center;
			}
		}
	}
}
