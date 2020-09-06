// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Threading;
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

			this.camera = new PerspectiveCamera(new Point3D(0, 0, -3), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
			this.Viewport.Camera = this.camera;

			this.Viewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });

			////Anamnesis.Quaternion rootrot = Module.SkeletonViewModel.GetBone("Root").RootRotation;
			////this.root.Transform = new RotateTransform3D(new QuaternionRotation3D(new Quaternion(rootrot.X, rootrot.Y, rootrot.Z, rootrot.W)));
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			SkeletonViewModel vm = this.DataContext as SkeletonViewModel;

			if (vm == null)
				return;

			////this.Viewport.Children.Add(vm.Root);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			SkeletonViewModel vm = this.DataContext as SkeletonViewModel;
			////this.Viewport.Children.Remove(vm.Root);
		}
	}
}
