// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using Anamnesis.Actor.Extensions;
using Anamnesis.Memory;
using Anamnesis.Posing.Visuals;
using Anamnesis.Services;
using MaterialDesignThemes.Wpf;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media.Media3D;
using XivToolsWpf.Math3D;
using XivToolsWpf.Math3D.Extensions;
using CmQuaternion = System.Numerics.Quaternion;
using CmVector = System.Numerics.Vector3;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

[AddINotifyPropertyChangedInterface]
public class BoneVisual3d : ModelVisual3D, ITransform, IBone, IDisposable
{
	public readonly List<TransformMemory> TransformMemories = new();

	private static bool scaleLinked = true;

	private readonly object transformLock = new();
	private readonly QuaternionRotation3D rotation;
	private readonly TranslateTransform3D position;
	private BoneTargetVisual3d? targetVisual;
	private BoneVisual3d? parent;
	private Line? lineToParent;

	public BoneVisual3d(SkeletonVisual3d skeleton, string name)
	{
		this.Skeleton = skeleton;

		this.rotation = new QuaternionRotation3D();

		RotateTransform3D rot = new() { Rotation = this.rotation };
		this.position = new TranslateTransform3D();

		Transform3DGroup transformGroup = new();
		transformGroup.Children.Add(rot);
		transformGroup.Children.Add(this.position);

		this.Transform = transformGroup;

		PaletteHelper ph = new();
		ITheme t = ph.GetTheme();

		this.targetVisual = new BoneTargetVisual3d(this);
		this.Children.Add(this.targetVisual);

		this.BoneName = name;
	}

	public SkeletonVisual3d Skeleton { get; private set; }
	public TransformMemory TransformMemory => this.TransformMemories[0];

	public bool IsEnabled { get; set; } = true;
	public string BoneName { get; set; }
	public List<BoneVisual3d> LinkedBones { get; set; } = new();
	public int LinkedBonesCount => this.LinkedBones.Count;
	public virtual string TooltipKey => "Pose_" + this.BoneName;
	public bool IsTransformLocked { get; set; } = false;

	public bool CanRotate => PoseService.Instance.FreezeRotation && !this.IsTransformLocked;
	public CmQuaternion Rotation { get; set; }
	public bool CanScale => PoseService.Instance.FreezeScale && !this.IsTransformLocked;
	public CmVector Scale { get; set; }
	public bool CanTranslate => PoseService.Instance.FreezePositions && !this.IsTransformLocked;
	public CmVector Position { get; set; }

	public bool IsAttachmentBone
	{
		get
		{
			return this.BoneName == "n_buki_r" ||
				this.BoneName == "n_buki_l" ||
				this.BoneName == "j_buki_sebo_r" ||
				this.BoneName == "j_buki_sebo_l";
		}
	}

	public bool CanLinkScale => !this.IsAttachmentBone;

	public bool ScaleLinked
	{
		get
		{
			if (this.IsAttachmentBone)
				return true;

			return scaleLinked;
		}

		set => scaleLinked = value;
	}

	public bool EnableLinkedBones
	{
		get
		{
			if (this.LinkedBonesCount <= 0)
				return false;

			return SettingsService.Current.PosingBoneLinks.Get(this.BoneName, true);
		}

		set
		{
			SettingsService.Current.PosingBoneLinks.Set(this.BoneName, value);

			foreach (BoneVisual3d link in this.LinkedBones)
			{
				SettingsService.Current.PosingBoneLinks.Set(link.BoneName, value);
			}
		}
	}

	public string Tooltip
	{
		get
		{
			string? customName = CustomBoneNameService.GetBoneName(this.BoneName);

			if (!string.IsNullOrEmpty(customName))
				return customName;

			string str = LocalizationService.GetString(this.TooltipKey, true);

			if (string.IsNullOrEmpty(str))
				return this.BoneName;

			return str;
		}

		set
		{
			if (string.IsNullOrEmpty(value) || LocalizationService.GetString(this.TooltipKey, true) == value)
			{
				CustomBoneNameService.SetBoneName(this.BoneName, null);
			}
			else
			{
				CustomBoneNameService.SetBoneName(this.BoneName, value);
			}
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

			return rot * this.Parent.TransformMemory.Rotation;
		}
	}

	public BoneVisual3d? Visual => this;

	public void Dispose()
	{
		this.Children.Clear();
		this.targetVisual?.Dispose();
		this.targetVisual = null;

		this.parent?.Children.Remove(this);

		if (this.lineToParent != null)
		{
			this.parent?.Children.Remove(this.lineToParent);
			this.lineToParent.Dispose();
			this.lineToParent = null;
		}

		this.parent = null;
	}

	public virtual void Synchronize()
	{
		foreach (TransformMemory transformMemory in this.TransformMemories)
			transformMemory.Synchronize();

		this.ReadTransform();
	}

	public virtual void ReadTransform(bool readChildren = false, Dictionary<string, Transform>? snapshot = null)
	{
		if (!this.IsEnabled)
			return;

		lock (this.transformLock)
		{
			Transform newTransform;

			// Use the snapshot if provided
			if (snapshot != null && snapshot.TryGetValue(this.BoneName, out var transform))
			{
				newTransform = transform;
			}
			else
			{
				newTransform = new Transform
				{
					Position = this.TransformMemory.Position,
					Rotation = this.TransformMemory.Rotation,
					Scale = this.TransformMemory.Scale,
				};
			}

			// Convert the character-relative transform into a parent-relative transform
			if (this.Parent != null)
			{
				Transform parentTransform;
				if (snapshot != null && snapshot.TryGetValue(this.Parent.BoneName, out var parentSnapshot))
				{
					parentTransform = parentSnapshot;
				}
				else
				{
					parentTransform = new Transform
					{
						Position = this.Parent.TransformMemory.Position,
						Rotation = this.Parent.TransformMemory.Rotation,
						Scale = this.Parent.TransformMemory.Scale,
					};
				}

				CmVector parentPosition = parentTransform.Position;
				CmQuaternion parentRot = CmQuaternion.Normalize(parentTransform.Rotation);
				parentRot = CmQuaternion.Inverse(parentRot);

				// Relative position
				newTransform.Position -= parentPosition;

				// Unrotate bones, since we will transform them ourselves.
				Matrix4x4 rotMatrix = Matrix4x4.CreateFromQuaternion(parentRot);
				newTransform.Position = CmVector.Transform(newTransform.Position, rotMatrix);

				// Relative rotation
				newTransform.Rotation = CmQuaternion.Normalize(CmQuaternion.Multiply(parentRot, newTransform.Rotation));
			}

			// Apply the updates in a single step
			this.Position = newTransform.Position;
			this.Rotation = newTransform.Rotation;
			this.Scale = newTransform.Scale;

			// Set the Media3D hierarchy transforms
			this.rotation.Quaternion = newTransform.Rotation.ToMedia3DQuaternion();
			this.position.OffsetX = newTransform.Position.X;
			this.position.OffsetY = newTransform.Position.Y;
			this.position.OffsetZ = newTransform.Position.Z;

			// Draw a line for visualization
			if (this.Parent != null && this.lineToParent != null)
			{
				var endPoint = this.lineToParent.Points[1];
				endPoint.X = newTransform.Position.X;
				endPoint.Y = newTransform.Position.Y;
				endPoint.Z = newTransform.Position.Z;
				this.lineToParent.Points[1] = endPoint;
			}

			if (readChildren)
			{
				foreach (Visual3D child in this.Children)
				{
					if (child is BoneVisual3d childBone)
					{
						childBone.ReadTransform(true, snapshot);
					}
				}
			}
		}
	}

	public virtual void WriteTransform(ModelVisual3D root, bool writeChildren = true, bool writeLinked = true)
	{
		if (!this.IsEnabled)
			return;

		lock (this.transformLock)
		{
			foreach (TransformMemory transformMemory in this.TransformMemories)
			{
				transformMemory.EnableReading = false;
			}

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
			bool changed = false;
			foreach (TransformMemory transformMemory in this.TransformMemories)
			{
				if (this.CanTranslate && !transformMemory.Position.IsApproximately(position))
				{
					transformMemory.Position = position;
					changed = true;
				}

				if (this.CanScale && !transformMemory.Scale.IsApproximately(this.Scale))
				{
					transformMemory.Scale = this.Scale;
					changed = true;
				}

				if (this.CanRotate)
				{
					CmQuaternion newRot = rotation.FromMedia3DQuaternion();
					if (!transformMemory.Rotation.IsApproximately(newRot))
					{
						transformMemory.Rotation = newRot;
						changed = true;
					}
				}
			}

			if (changed)
			{
				if (writeLinked && this.EnableLinkedBones)
				{
					foreach (BoneVisual3d link in this.LinkedBones)
					{
						link.Rotation = this.Rotation;
						link.WriteTransform(root, writeChildren, false);
					}
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

			foreach (TransformMemory transformMemory in this.TransformMemories)
			{
				transformMemory.EnableReading = true;
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
