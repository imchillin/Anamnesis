// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.ThreeD;

	using CmQuaternion = ConceptMatrix.Quaternion;
	using CmTransform = ConceptMatrix.Transform;
	using CmVector = ConceptMatrix.Vector;

	public class Bone : ModelVisual3D, INotifyPropertyChanged, IDisposable
	{
		public readonly SkeletonService.Bone Definition;
		private readonly IMemory<CmTransform> transformMem;

		private readonly Sphere sphere;
		private readonly RotateTransform3D rotation;
		private readonly ScaleTransform3D scale;
		private readonly TranslateTransform3D position;
		private Bone parent;
		private Line lineToParent;
		private Quaternion initialRotation;

		public Bone(string name, IMemory<CmTransform> transformMem, SkeletonService.Bone definition)
		{
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
			set => this.transformMem.Value = value;
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

		public void ReadTransform()
		{
			if (!this.IsEnabled)
				return;

			this.Position = this.LiveTransform.Position;
			this.Rotation = this.LiveTransform.Rotation;
			this.Scale = this.LiveTransform.Scale;

			/*
				Bones are retrieved from FFXIV with character-relative positions, scale, and roation.
				Since we are creating a hierarchy, we need to convert the transforms to relative transforms.

				once the relative rotation has been calculated, the positions need to be un-rotated.
			*/

			Point3D position = this.Position.ToMedia3DPoint();
			Vector3D scale = this.Scale.ToMedia3DVector();
			this.initialRotation = this.Rotation.ToMedia3DQuaternion();

			if (this.Parent != null)
			{
				CmTransform parentTransform = this.Parent.LiveTransform;
				Point3D parentPosition = parentTransform.Position.ToMedia3DPoint();
				Vector3D parentScale = parentTransform.Scale.ToMedia3DVector();

				position = (Point3D)(position - parentPosition);
				////relativeScale *= this.Parent.LiveTransform.Scale;

				////rotation.Invert();
				////rotation = parentTransform.Rotation.ToMedia3DQuaternion() * rotation;
			}

			this.rotation.Rotation = new QuaternionRotation3D(Quaternion.Identity);
			this.position.OffsetX = position.X;
			this.position.OffsetY = position.Y;
			this.position.OffsetZ = position.Z;
			////this.scale.ScaleX = scale.X;
			////this.scale.ScaleY = scale.Y;
			////this.scale.ScaleZ = scale.Z;

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

		public void WriteTransform(ModelVisual3D root)
		{
			if (!this.IsEnabled)
				return;

			this.scale.ScaleX = this.Scale.X;
			this.scale.ScaleY = this.Scale.Y;
			this.scale.ScaleZ = this.Scale.Z;

			// TODO: since we are caluclating rotation manually (not from the visual hierarchy)
			// we must also calcualte all child rotations manually...
			Quaternion newRotation = this.Rotation.ToMedia3DQuaternion();
			Quaternion initialInv = this.initialRotation;
			initialInv.Invert();
			Quaternion delta = newRotation * initialInv;
			this.rotation.Rotation = new QuaternionRotation3D(delta);

			GeneralTransform3D transform = this.TransformToAncestor(root);
			Point3D position = transform.Transform(new Point3D(0, 0, 0));
			////Vector3D scale = transform.Transform(new Point3D(1, 1, 1)) - position;

			CmTransform live = this.LiveTransform;
			live.Position = position.ToCmVector();
			////live.Scale = scale.ToCmVector();
			live.Rotation = this.Rotation;

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
			this.transformMem.Dispose();
		}
	}
}
