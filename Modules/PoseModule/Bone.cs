// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.ComponentModel;
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

		public Quaternion Rotation { get; set; }
		public Vector Scale { get; set; }
		public Vector Position { get; set; }

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

		public void WriteTransform(ModelVisual3D root)
		{
			if (!this.IsEnabled)
				return;

			GeneralTransform3D transform = this.TransformToAncestor(root);

			Point3D position = transform.Transform(new Point3D(0, 0, 0));
			Vector3D scale = transform.Transform(new Point3D(1, 1, 1)) - position;

			CmTransform live = this.LiveTransform;
			live.Position = new CmVector((float)position.X, (float)position.Y, (float)position.Z);
			live.Scale = new CmVector((float)scale.X, (float)scale.Y, (float)scale.Z);

			this.LiveTransform = live;
		}

		public void Dispose()
		{
			this.transformMem.Dispose();
		}
	}
}
