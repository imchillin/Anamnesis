// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Animation;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.PoseModule.Extensions;
	using ConceptMatrix.ThreeD;

	using CmQuaternion = ConceptMatrix.Quaternion;
	using CmTransform = ConceptMatrix.Transform;
	using CmVector = ConceptMatrix.Vector;

	public class Bone : ModelVisual3D, INotifyPropertyChanged, IDisposable
	{
		public readonly SkeletonViewModel Skeleton;
		public readonly SkeletonService.Bone Definition;
		private readonly IMemory<CmTransform> transformMem;

		private readonly Sphere sphere;
		private readonly RotateTransform3D rotation;
		private readonly ScaleTransform3D scale;
		private readonly TranslateTransform3D position;
		private Bone parent;
		private Line lineToParent;

		public Bone(SkeletonViewModel skeleton, string name, IMemory<CmTransform> transformMem, SkeletonService.Bone definition)
		{
			this.Skeleton = skeleton;
			this.Definition = definition;
			this.BoneName = name;
			this.transformMem = transformMem;

			this.Definition = definition;
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
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string BoneName { get; private set; }
		public bool IsEnabled { get; set; } = true;

		public CmQuaternion Rotation { get; set; }
		public CmVector Scale { get; set; }
		public CmVector Position { get; set; }

		public string Tooltip
		{
			get
			{
				return this.BoneName;
			}
		}

		public CmTransform LiveTransform
		{
			get => this.transformMem.Value;
			set => this.transformMem.SetValue(value, true);
		}

		public Bone Parent
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
					this.lineToParent = new Line();
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

				return rot * this.Parent.LiveTransform.Rotation;
			}
		}

		public void ReadTransform()
		{
			if (!this.IsEnabled)
				return;

			this.Position = this.LiveTransform.Position;
			this.Rotation = this.LiveTransform.Rotation;
			this.Scale = this.LiveTransform.Scale;

			// Convert the character-relative transform into a parent-relative transform
			Point3D position = this.Position.ToMedia3DPoint();
			Vector3D scale = this.Scale.ToMedia3DVector();
			Quaternion rotation = this.Rotation.ToMedia3DQuaternion();

			if (this.Parent != null)
			{
				CmTransform parentTransform = this.Parent.LiveTransform;
				Point3D parentPosition = parentTransform.Position.ToMedia3DPoint();
				Vector3D parentScale = parentTransform.Scale.ToMedia3DVector();
				Quaternion parentRot = parentTransform.Rotation.ToMedia3DQuaternion();
				parentRot.Invert();

				// relative position
				position = (Point3D)(position - parentPosition);

				// relative rotation
				rotation = parentRot * rotation;

				// relative scale
				scale = (scale - parentScale) + new Vector3D(1, 1, 1);

				// unrotate bones, since we will transform them ourselves.
				RotateTransform3D rotTrans = new RotateTransform3D(new QuaternionRotation3D(parentRot));
				position = rotTrans.Transform(position);
			}

			// Store the new parent-relative transform info
			this.Position = position.ToCmVector();
			this.Rotation = rotation.ToCmQuaternion();
			this.Scale = scale.ToCmVector();

			// Set the Media3D hierarchy transforms
			this.rotation.Rotation = new QuaternionRotation3D(rotation);
			this.position.OffsetX = position.X;
			this.position.OffsetY = position.Y;
			this.position.OffsetZ = position.Z;
			this.scale.ScaleX = scale.X;
			this.scale.ScaleY = scale.Y;
			this.scale.ScaleZ = scale.Z;

			// Draw a line for visualization
			if (this.Parent != null)
			{
				Point3D p = this.lineToParent.Points[1];
				p.X = position.X;
				p.Y = position.Y;
				p.Z = position.Z;
				this.lineToParent.Points[1] = p;
			}
		}

		public void WriteTransform(ModelVisual3D root)
		{
			if (!this.IsEnabled)
				return;

			// Apply the current values to the visual tree
			this.rotation.Rotation = new QuaternionRotation3D(this.Rotation.ToMedia3DQuaternion());
			this.position.OffsetX = this.Position.X;
			this.position.OffsetY = this.Position.Y;
			this.position.OffsetZ = this.Position.Z;
			this.scale.ScaleX = this.Scale.X;
			this.scale.ScaleY = this.Scale.Y;
			this.scale.ScaleZ = this.Scale.Z;

			// convert the values in the tree to character-relative space
			MatrixTransform3D transform = (MatrixTransform3D)this.TransformToAncestor(root);
			CmVector position = default;

			Vector3D scale = this.Scale.ToMedia3DVector(); // ??

			Quaternion rotation = transform.Matrix.ToQuaternion();
			rotation.Invert();

			position.X = (float)transform.Matrix.OffsetX;
			position.Y = (float)transform.Matrix.OffsetY;
			position.Z = (float)transform.Matrix.OffsetZ;

			// and push those values to the game memory
			CmTransform live = this.LiveTransform;
			live.Position = position;
			live.Scale = scale.ToCmVector();
			live.Rotation = rotation.ToCmQuaternion();
			this.LiveTransform = live;

			foreach (Visual3D child in this.Children)
			{
				if (child is Bone childBone)
				{
					childBone.WriteTransform(root);
				}
			}
		}

		public void Dispose()
		{
			if (this.Parent != null)
				this.Parent.Children.Remove(this);

			this.transformMem.Dispose();
		}
	}
}
