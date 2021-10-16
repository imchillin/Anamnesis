// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Media.Media3D;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Extensions;
	using Anamnesis.Services;
	using MaterialDesignThemes.Wpf;
	using PropertyChanged;
	using XivToolsWpf.Meida3D;

	using CmQuaternion = Anamnesis.Memory.Quaternion;
	using CmVector = Anamnesis.Memory.Vector;
	using Quaternion = System.Windows.Media.Media3D.Quaternion;

	[AddINotifyPropertyChangedInterface]
	public class BoneVisual3d : ModelVisual3D, ITransform, IBone
	{
		private readonly QuaternionRotation3D rotation;
		private readonly TranslateTransform3D position;

		private BoneVisual3d? parent;
		private Line? lineToParent;

		public BoneVisual3d(TransformMemory transform, SkeletonVisual3d skeleton, string name)
		{
			this.ViewModel = transform;
			this.Skeleton = skeleton;

			this.rotation = new QuaternionRotation3D();

			RotateTransform3D rot = new RotateTransform3D();
			rot.Rotation = this.rotation;
			this.position = new TranslateTransform3D();

			Transform3DGroup transformGroup = new Transform3DGroup();
			transformGroup.Children.Add(rot);
			transformGroup.Children.Add(this.position);

			this.Transform = transformGroup;

			PaletteHelper ph = new PaletteHelper();
			ITheme t = ph.GetTheme();

			this.OriginalBoneName = name;
			this.BoneName = name;
		}

		public SkeletonVisual3d Skeleton { get; private set; }
		public TransformMemory ViewModel { get; set; }

		public bool IsEnabled { get; set; } = true;
		public string OriginalBoneName { get; set; }
		public string BoneName { get; set; }

		public bool CanRotate => PoseService.Instance.FreezeRotation;
		public CmQuaternion Rotation { get; set; }
		public bool CanScale => PoseService.Instance.FreezeScale;
		public CmVector Scale { get; set; }
		public bool CanTranslate => PoseService.Instance.FreezePositions;
		public CmVector Position { get; set; }

		public BoneVisual3d? LinkedEye { get; set; }

		public virtual string TooltipKey => "Pose_" + this.BoneName;

		public string Tooltip
		{
			get
			{
				string str = LocalizationService.GetString(this.TooltipKey, true);

				if (string.IsNullOrEmpty(str))
					return this.BoneName;

				return str;
			}
		}

		public BoneVisual3d? Parent
		{
			get
			{
				return this.parent;
			}

			set
			{
				if (this.parent != null)
				{
					this.parent.Children.Remove(this);
					this.parent.Children.Remove(this.lineToParent);
				}

				if (this.Skeleton.Children.Contains(this))
					this.Skeleton.Children.Remove(this);

				this.parent = value;

				if (this.parent != null)
				{
					if (this.lineToParent == null)
					{
						this.lineToParent = new Line();
						System.Windows.Media.Color c = default;
						c.R = 128;
						c.G = 128;
						c.B = 128;
						c.A = 255;
						this.lineToParent.Color = c;
						this.lineToParent.Points.Add(new Point3D(0, 0, 0));
						this.lineToParent.Points.Add(new Point3D(0, 0, 0));
					}

					this.parent.Children.Add(this);
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

		public Vector3D WorldPosition
		{
			get
			{
				GeneralTransform3D trans = this.TransformToAncestor(this.Skeleton);

				Point3D p;
				trans.TryTransform(default, out p);
				return (Vector3D)p;
			}
		}

		public BoneVisual3d? Visual => this;

		public virtual void ReadTransform(bool readChildren = false)
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
				TransformMemory parentTransform = this.Parent.ViewModel;
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
			this.rotation.Quaternion = rotation;
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

			if (readChildren)
			{
				foreach (Visual3D child in this.Children)
				{
					if (child is BoneVisual3d childBone)
					{
						childBone.ReadTransform(true);
					}
				}
			}
		}

		public virtual void WriteTransform(ModelVisual3D root, bool writeChildren = true)
		{
			if (!this.IsEnabled)
				return;

			// Apply the current values to the visual tree
			this.rotation.Quaternion = this.Rotation.ToMedia3DQuaternion();
			this.position.OffsetX = this.Position.X;
			this.position.OffsetY = this.Position.Y;
			this.position.OffsetZ = this.Position.Z;

			// convert the values in the tree to character-relative space
			MatrixTransform3D transform;

			try
			{
				transform = (MatrixTransform3D)this.TransformToAncestor(root);
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to transform bone: {this.BoneName} to root", ex);
			}

			Quaternion rotation = transform.Matrix.ToQuaternion();
			rotation.Invert();

			CmVector position = default;
			position.X = (float)transform.Matrix.OffsetX;
			position.Y = (float)transform.Matrix.OffsetY;
			position.Z = (float)transform.Matrix.OffsetZ;

			// and push those values to the game memory
			if (PoseService.Instance.FreezePositions)
				this.ViewModel.Position = position;

			if (PoseService.Instance.FreezeScale)
				this.ViewModel.Scale = this.Scale;

			if (PoseService.Instance.FreezeRotation)
				this.ViewModel.Rotation = rotation.ToCmQuaternion();

			if (this.LinkedEye != null && this.Skeleton.LinkEyes)
			{
				this.LinkedEye.ViewModel.Rotation = this.ViewModel.Rotation;
				this.LinkedEye.Rotation = this.Rotation;
			}

			if (writeChildren)
			{
				foreach (Visual3D child in this.Children)
				{
					if (child is BoneVisual3d childBone)
					{
						if (PoseService.Instance.EnableParenting)
						{
							childBone.WriteTransform(root);
						}
						else
						{
							childBone.ReadTransform(true);
						}
					}
				}
			}
		}

		public void GetChildren(ref List<BoneVisual3d> bones)
		{
			foreach (Visual3D? child in this.Children)
			{
				if (child is BoneVisual3d childBoneVisual)
				{
					bones.Add(childBoneVisual);
					childBoneVisual.GetChildren(ref bones);
				}
			}
		}

		public bool HasParent(BoneVisual3d target)
		{
			if (this.parent == null)
				return false;

			if (this.parent == target)
				return true;

			return this.parent.HasParent(target);
		}

		public override string ToString()
		{
			return base.ToString() + "(" + this.BoneName + ")";
		}
	}
}