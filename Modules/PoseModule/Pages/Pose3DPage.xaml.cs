// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.ThreeD;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	public partial class Pose3DPage : UserControl
	{
		public Pose3DPage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = Module.SkeletonViewModel;

			this.Viewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -3), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
			this.Viewport.Children.Add(Module.SkeletonViewModel.Root);

			////ConceptMatrix.Quaternion rootrot = Module.SkeletonViewModel.GetBone("Root").RootRotation;
			////this.root.Transform = new RotateTransform3D(new QuaternionRotation3D(new Quaternion(rootrot.X, rootrot.Y, rootrot.Z, rootrot.W)));
		}

		[SuppressPropertyChangedWarnings]
		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.IsVisible)
			{
				// Watch camera thread
				new Thread(new ThreadStart(this.WatchCamera)).Start();
			}
		}

		private void WatchCamera()
		{
			IMemory<float> camX = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraAngleX);
			IMemory<float> camY = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraAngleY);
			IMemory<float> camZ = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraRotation);

			Vector3D camEuler = default;

			bool vis = true;
			while (vis && Application.Current != null)
			{
				camEuler.Y = (float)MathUtils.RadiansToDegrees((double)camX.Value) - 180;
				camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)camY.Value);
				camEuler.X = (float)MathUtils.RadiansToDegrees((double)camZ.Value);
				Quaternion q = camEuler.ToQuaternion();

				try
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						vis = this.IsVisible; ////&& this.IsEnabled;
						Transform3DGroup g = new Transform3DGroup();
						g.Children.Add(new RotateTransform3D(new QuaternionRotation3D(q)));
						g.Children.Add(new TranslateTransform3D(0, 0.75, 0));
						this.Viewport.Camera.Transform = g;
					});
				}
				catch (Exception)
				{
				}

				Thread.Sleep(16);
			}
		}
	}
}
