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

	using CmQuaternion = ConceptMatrix.Quaternion;
	using CmTransform = ConceptMatrix.Transform;
	using CmVector = ConceptMatrix.Vector;

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

			this.Viewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -3), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);

			this.root = new ModelVisual3D();

			foreach (Bone bone in Module.SkeletonViewModel.Bones)
			{
				this.GetGizmo(bone);
			}

			this.Viewport.Children.Add(this.root);

			foreach (BoneGizmo gizmo in this.gizmoLookup.Values)
			{
				gizmo.ReadTransform();
			}

			////ConceptMatrix.Quaternion rootrot = Module.SkeletonViewModel.GetBone("Root").RootRotation;
			////this.root.Transform = new RotateTransform3D(new QuaternionRotation3D(new Quaternion(rootrot.X, rootrot.Y, rootrot.Z, rootrot.W)));
		}

		private BoneGizmo GetGizmo(Bone bone)
		{
			if (this.gizmoLookup.ContainsKey(bone))
				return this.gizmoLookup[bone];

			BoneGizmo parent = null;
			if (bone.Parent != null)
				parent = this.GetGizmo(bone.Parent);

			BoneGizmo boneGizmo = new BoneGizmo(bone.BoneName, bone.TransformMem, bone.Definition, parent);

			if (parent != null)
			{
				parent.Children.Add(boneGizmo);
			}
			else
			{
				this.root.Children.Add(boneGizmo);
			}

			boneGizmo.IsEnabled = bone.IsEnabled;
			this.gizmoLookup.Add(bone, boneGizmo);
			return boneGizmo;
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

						// ugh, must be done from ui thread...
						foreach (BoneGizmo gizmo in this.gizmoLookup.Values)
						{
							gizmo.ReadTransform();
						}
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
			public readonly SkeletonService.Bone Definition;
			public readonly BoneGizmo Parent;

			private readonly IMemory<CmTransform> transformMem;

			private readonly Sphere sphere;
			private readonly Line lineToParent;
			private readonly RotateTransform3D rotation;
			private readonly ScaleTransform3D scale;
			private readonly TranslateTransform3D position;

			public BoneGizmo(string name, IMemory<CmTransform> transformMem, SkeletonService.Bone definition, BoneGizmo parent)
			{
				this.Definition = definition;
				this.Parent = parent;
				this.BoneName = name;
				this.transformMem = transformMem;

				this.rotation = new RotateTransform3D();
				this.scale = new ScaleTransform3D();
				this.position = new TranslateTransform3D();

				Transform3DGroup transformGroup = new Transform3DGroup();
				transformGroup.Children.Add(this.rotation);
				transformGroup.Children.Add(this.scale);
				transformGroup.Children.Add(this.position);
				this.Transform = transformGroup;

				this.sphere = new Sphere();
				this.sphere.Radius = 0.01;
				this.sphere.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
				this.Children.Add(this.sphere);

				if (this.Parent != null)
				{
					this.lineToParent = new Line();
					this.lineToParent.Points.Add(new Point3D(0, 0, 0));
					this.lineToParent.Points.Add(new Point3D(0, 0, 0));
					this.Parent.Children.Add(this.lineToParent);
				}
			}

			public string BoneName { get; private set; }
			public bool IsEnabled { get; set; } = true;

			public CmTransform LiveTransform
			{
				get => this.transformMem.Value;
				set => this.transformMem.Value = value;
			}

			public void ReadTransform()
			{
				if (!this.IsEnabled)
					return;

				CmVector relativePos = this.LiveTransform.Position;
				CmQuaternion relativeRot = this.LiveTransform.Rotation;
				CmVector relativeScale = this.LiveTransform.Scale;

				if (this.Parent != null)
				{
					relativePos -= this.Parent.LiveTransform.Position;
					////relativeScale *= this.Parent.LiveTransform.Scale;

					relativeRot.Invert();
					relativeRot = this.Parent.LiveTransform.Rotation * relativeRot;
				}

				////this.rotation.Rotation = new QuaternionRotation3D(new Quaternion(relativeRot.X, relativeRot.Y, relativeRot.Z, relativeRot.W));
				this.position.OffsetX = relativePos.X;
				this.position.OffsetY = relativePos.Y;
				this.position.OffsetZ = relativePos.Z;
				this.scale.ScaleX = relativeScale.X;
				this.scale.ScaleY = relativeScale.Y;
				this.scale.ScaleZ = relativeScale.Z;

				// TODO: update this naturally
				if (this.Parent != null)
				{
					CmVector parentPos = this.LiveTransform.Position - this.Parent.LiveTransform.Position;

					Point3D p = this.lineToParent.Points[1];
					p.X = parentPos.X;
					p.Y = parentPos.Y;
					p.Z = parentPos.Z;
					this.lineToParent.Points[1] = p;
				}
			}
		}
	}
}
