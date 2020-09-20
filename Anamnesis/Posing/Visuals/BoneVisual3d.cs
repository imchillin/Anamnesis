// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Extensions;
	using Anamnesis.ThreeD;
	using MaterialDesignThemes.Wpf;
	using PropertyChanged;

	using CmQuaternion = Anamnesis.Memory.Quaternion;
	using CmTransform = Anamnesis.Memory.Transform;
	using CmVector = Anamnesis.Memory.Vector;
	using Quaternion = System.Windows.Media.Media3D.Quaternion;

	[AddINotifyPropertyChangedInterface]
	public class BoneVisual3d : ModelVisual3D
	{
		private readonly RotateTransform3D rotation;
		private readonly TranslateTransform3D position;

		private BoneVisual3d? parent;
		private Line? lineToParent;

		public BoneVisual3d(TransformViewModel transform, SkeletonVisual3d skeleton)
		{
			this.ViewModel = transform;
			this.Skeleton = skeleton;

			this.rotation = new RotateTransform3D();
			this.position = new TranslateTransform3D();

			Transform3DGroup transformGroup = new Transform3DGroup();
			transformGroup.Children.Add(this.rotation);
			transformGroup.Children.Add(this.position);

			this.Transform = transformGroup;

			PaletteHelper ph = new PaletteHelper();

			Sphere sphere = new Sphere();
			sphere.Radius = 0.005;
			System.Windows.Media.Color c1 = System.Windows.Media.Color.FromArgb(200, 255, 255, 255);
			sphere.Material = new EmissiveMaterial(new SolidColorBrush(c1));
			this.Children.Add(sphere);
		}

		public SkeletonVisual3d Skeleton { get; private set; }
		public TransformViewModel ViewModel { get; private set; }

		public bool IsEnabled { get; set; } = true;

		public CmQuaternion Rotation { get; set; }
		public CmVector Scale { get; set; }
		public CmVector Position { get; set; }

		public BoneVisual3d? Parent
		{
			get
			{
				return this.parent;
			}

			set
			{
				this.parent = value;

				if (this.parent != null)
				{
					this.parent.Children.Add(this);

					this.lineToParent = new Line();

					System.Windows.Media.Color c = default;
					c.R = 128;
					c.G = 128;
					c.B = 128;
					c.A = 255;
					this.lineToParent.Color = c;
					this.lineToParent.Points.Add(new Point3D(0, 0, 0));
					this.lineToParent.Points.Add(new Point3D(0, 0, 0));
					this.parent.Children.Add(this.lineToParent);
				}
			}
		}

		public CmQuaternion RootRotation
		{
			get
			{
				CmQuaternion rot = this.Skeleton.RootRotation;

				if (this.Parent == null)
					return rot;

				return rot * this.Parent.ViewModel.Rotation;
			}
		}

		public void ReadTransform()
		{
			if (!this.IsEnabled)
				return;

			this.Position = this.ViewModel.Position;
			this.Rotation = this.ViewModel.Rotation;
			this.Scale = this.ViewModel.Scale;

			// Convert the character-relative transform into a parent-relative transform
			Point3D position = this.Position.ToMedia3DPoint();
			Quaternion rotation = this.Rotation.ToMedia3DQuaternion();

			if (this.Parent != null)
			{
				TransformViewModel parentTransform = this.Parent.ViewModel;
				Point3D parentPosition = parentTransform.Position.ToMedia3DPoint();
				Quaternion parentRot = parentTransform.Rotation.ToMedia3DQuaternion();
				parentRot.Invert();

				// relative position
				position = (Point3D)(position - parentPosition);

				// relative rotation
				rotation = parentRot * rotation;

				// unrotate bones, since we will transform them ourselves.
				RotateTransform3D rotTrans = new RotateTransform3D(new QuaternionRotation3D(parentRot));
				position = rotTrans.Transform(position);
			}

			// Store the new parent-relative transform info
			this.Position = position.ToCmVector();
			this.Rotation = rotation.ToCmQuaternion();

			// Set the Media3D hierarchy transforms
			this.rotation.Rotation = new QuaternionRotation3D(rotation);
			this.position.OffsetX = position.X;
			this.position.OffsetY = position.Y;
			this.position.OffsetZ = position.Z;

			// Draw a line for visualization
			if (this.Parent != null && this.lineToParent != null)
			{
				Point3D p = this.lineToParent.Points[1];
				p.X = position.X;
				p.Y = position.Y;
				p.Z = position.Z;
				this.lineToParent.Points[1] = p;
			}
		}

		public void WriteTransform(ModelVisual3D root, bool writeChildren = true)
		{
			if (!this.IsEnabled)
				return;

			// Apply the current values to the visual tree
			this.rotation.Rotation = new QuaternionRotation3D(this.Rotation.ToMedia3DQuaternion());
			this.position.OffsetX = this.Position.X;
			this.position.OffsetY = this.Position.Y;
			this.position.OffsetZ = this.Position.Z;

			// convert the values in the tree to character-relative space
			MatrixTransform3D transform = (MatrixTransform3D)this.TransformToAncestor(root);

			Quaternion rotation = transform.Matrix.ToQuaternion();
			rotation.Invert();

			CmVector position = default;
			position.X = (float)transform.Matrix.OffsetX;
			position.Y = (float)transform.Matrix.OffsetY;
			position.Z = (float)transform.Matrix.OffsetZ;

			// and push those values to the game memory
			this.ViewModel.Position = position;
			this.ViewModel.Scale = this.Scale;
			this.ViewModel.Rotation = rotation.ToCmQuaternion();

			if (writeChildren)
			{
				foreach (Visual3D child in this.Children)
				{
					if (child is BoneVisual3d childBone)
					{
						childBone.WriteTransform(root);
					}
				}
			}
		}
	}
}
