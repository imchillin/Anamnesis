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
		private Dictionary<Bone, BoneGizmo> gizmoLookup = new Dictionary<Bone, BoneGizmo>();
		private ModelVisual3D root;

		public Pose3DPage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = Module.SkeletonViewModel;

			this.Viewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -2.5), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);

			this.root = new ModelVisual3D();

			foreach (Bone bone in Module.SkeletonViewModel.Bones)
			{
				this.GetGizmo(bone);
			}

			ConceptMatrix.Quaternion rootrot = Module.SkeletonViewModel.GetBone("Root").RootRotation;
			this.root.Transform = new RotateTransform3D(new QuaternionRotation3D(new Quaternion(rootrot.X, rootrot.Y, rootrot.Z, rootrot.W)));

			this.Viewport.Children.Add(this.root);
		}

		private BoneGizmo GetGizmo(Bone bone)
		{
			if (this.gizmoLookup.ContainsKey(bone))
				return this.gizmoLookup[bone];

			BoneGizmo boneGiz = new BoneGizmo();
			this.gizmoLookup.Add(bone, boneGiz);

			if (bone.Parent != null)
			{
				ConceptMatrix.Vector relativePos = bone.LivePosition - bone.Parent.LivePosition;
				boneGiz.Transform = new TranslateTransform3D(relativePos.X, relativePos.Y, relativePos.Z);

				Line line = new Line();
				line.Points.Add(new Point3D(0, 0, 0));
				line.Points.Add(new Point3D(relativePos.X, relativePos.Y, relativePos.Z));

				BoneGizmo parent = this.GetGizmo(bone.Parent);
				parent.Children.Add(boneGiz);
				parent.Children.Add(line);
			}
			else
			{
				boneGiz.Transform = new TranslateTransform3D(bone.LivePosition.X, bone.LivePosition.Y, bone.LivePosition.Z);
				this.root.Children.Add(boneGiz);
			}

			return boneGiz;
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
						g.Children.Add(new TranslateTransform3D(0, 1, 0));
						this.Viewport.Camera.Transform = g;
					});
				}
				catch (Exception)
				{
				}

				Thread.Sleep(16);
			}
		}

		private class BoneGizmo : ModelVisual3D
		{
			private readonly Sphere sphere;

			public BoneGizmo()
			{
				this.sphere = new Sphere();
				this.sphere.Radius = 0.01;
				////this.sphere.Transform = new RotateTransform3D(new AxisAngleRotation3D(axis, 90));
				this.sphere.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
				this.Children.Add(this.sphere);
			}
		}
	}
}
