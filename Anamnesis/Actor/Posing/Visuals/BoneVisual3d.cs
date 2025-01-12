// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing.Visuals;

using Anamnesis.Actor.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XivToolsWpf.Math3D;
using XivToolsWpf.Math3D.Extensions;

/// <summary>
/// Represents a 3D visual representation of a bone.
/// The bone is part of a <see cref="SkeletonVisual3D"/>.
/// </summary>
public class BoneVisual3D : ModelVisual3D, IDisposable
{
	private static readonly double SphereRadius = 0.015;
	private static readonly Material SelectedMaterial = CreateMaterial(Colors.Orange);
	private static readonly Material HoveredMaterial = CreateMaterial(Colors.DarkOrange);
	private static readonly Material NormalMaterial = CreateMaterial(Colors.White, 128);
	private readonly PrsTransform sphereTransform = new();

	private readonly Sphere sphere;
	private BoneVisual3D? parent;
	private Line? lineToParent;
	private Vector3? lastPosition;
	private System.Numerics.Quaternion? lastRotation;

	/// <summary>
	/// Initializes a new instance of the <see cref="BoneVisual3D"/> class.
	/// </summary>
	/// <param name="skeleton">The skeleton visual to which this bone belongs.</param>
	/// <param name="bone">The bone entity to visualize.</param>
	public BoneVisual3D(SkeletonVisual3D skeleton, BoneEntity bone)
	{
		this.Skeleton = skeleton;
		this.Bone = bone;

		this.Rotation = new QuaternionRotation3D(bone.Rotation.ToMedia3DQuaternion());

		RotateTransform3D rot = new() { Rotation = this.Rotation };
		this.Position = new TranslateTransform3D(bone.Position.ToMedia3DVector());

		Transform3DGroup transformGroup = new();
		transformGroup.Children.Add(rot);
		transformGroup.Children.Add(this.Position);

		this.Transform = transformGroup;

		PaletteHelper ph = new();
		ITheme t = ph.GetTheme();

		this.Initialize(bone);

		this.sphere = new Sphere
		{
			Radius = SphereRadius,
			Material = NormalMaterial,
			Transform = this.sphereTransform.Transform,
		};
		this.Children.Add(this.sphere);
	}

	/// <summary>Gets the rotation of the bone.</summary>
	/// <remarks>The rotation is parent-relative.</remarks>
	public QuaternionRotation3D Rotation { get; private set; }

	/// <summary>Gets the position of the bone.</summary>
	/// <remarks>The position is parent-relative.</remarks>
	public TranslateTransform3D Position { get; private set; }

	/// <summary>Gets the skeleton visual to which this bone belongs.</summary>
	public SkeletonVisual3D Skeleton { get; private set; }

	/// <summary>Gets the bone entity being visualized.</summary>
	public BoneEntity Bone { get; private set; }

	/// <summary>Gets or sets the parent bone visual.</summary>
	public BoneVisual3D? Parent
	{
		get => this.parent;

		set
		{
			if (this.parent != null)
			{
				this.parent.Children.Remove(this);
				this.parent.Children.Remove(this.lineToParent);
			}

			this.parent = value;

			if (this.parent != null)
			{
				this.lineToParent ??= new Line
				{
					Color = Color.FromArgb(255, 128, 128, 128),
					Points = new Point3DCollection
					{
						new Point3D(0, 0, 0),
						new Point3D(0, 0, 0),
					},
				};

				this.parent.Children.Add(this);
				this.parent.Children.Add(this.lineToParent);
			}
		}
	}

	/// <summary>Disposes the bone visual and its children.</summary>
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Updates the visual representation of the bone.</summary>
	/// <remarks>
	/// Use this method to update the bone's position and rotation in the visual tree.
	/// </remarks>
	public void Update()
	{
		if (this.Bone == null)
			return;

		var currentPosition = this.Bone.Position;
		var currentRotation = this.Bone.Rotation;
		bool positionChanged = currentPosition != this.lastPosition;
		bool rotationChanged = currentRotation != this.lastRotation;

		if (positionChanged)
		{
			this.lastPosition = currentPosition;
			this.Position.OffsetX = currentPosition.X;
			this.Position.OffsetY = currentPosition.Y;
			this.Position.OffsetZ = currentPosition.Z;
		}

		if (rotationChanged)
		{
			this.lastRotation = currentRotation;
			this.Rotation.Quaternion = currentRotation.ToMedia3DQuaternion();
		}

		if (positionChanged || rotationChanged)
		{
			// Redraw the line connecting this bone to its parent
			this.RedrawLineToParent();
		}

		// Handle child bones
		foreach (BoneVisual3D bone in this.Children.OfType<BoneVisual3D>())
		{
			bone.Update();
		}
	}

	public virtual void OnCameraUpdated(Pose3DView owner)
	{
		double scale = Math.Clamp(owner.CameraDistance * 0.5, 0.2, 1);
		this.sphereTransform.UniformScale = scale;
		this.sphere.Transform = this.sphereTransform.Transform;

		// Handle child bones
		foreach (BoneVisual3D bone in this.Children.OfType<BoneVisual3D>())
		{
			bone.OnCameraUpdated(owner);
		}
	}

	/// <summary>
	/// Updates the material of the bone based on its selection and hover state.
	/// </summary>
	internal void UpdateMaterial()
	{
		if (this.Bone.IsHovered)
			this.sphere.Material = HoveredMaterial;
		else if (this.Bone.IsSelected)
			this.sphere.Material = SelectedMaterial;
		else
			this.sphere.Material = NormalMaterial;

		// Handle child bones
		foreach (BoneVisual3D bone in this.Children.OfType<BoneVisual3D>())
		{
			bone.UpdateMaterial();
		}
	}

	/// <summary>
	/// Disposes the bone visual and its children.
	/// </summary>
	/// <param name="disposing">True if the object is being disposed; otherwise, false.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			this.Children.Clear();

			if (this.sphere != null)
			{
				this.sphere.Children.Clear();
				this.sphere.Content = null;
			}

			this.parent?.Children.Remove(this);

			if (this.lineToParent != null)
			{
				this.parent?.Children.Remove(this.lineToParent);
				this.lineToParent.Dispose();
				this.lineToParent = null;
			}

			this.parent = null;
		}
	}

	/// <summary>
	/// Creates a material with the specified color and alpha value.
	/// </summary>
	/// <param name="color">The color of the material.</param>
	/// <param name="alpha">The alpha value of the material.</param>
	/// <returns>The created material.</returns>
	private static Material CreateMaterial(Color color, byte alpha = 255)
	{
		color.A = alpha;
		return new DiffuseMaterial(new SolidColorBrush(color));
	}

	/// <summary>Initializes the visual representation of the bone.</summary>
	/// <param name="bone">The bone entity to initialize.</param>
	private void Initialize(BoneEntity bone)
	{
		this.Children.Clear();

		foreach (var childBone in bone.Children.OfType<BoneEntity>())
		{
			var childVisual = new BoneVisual3D(this.Skeleton, childBone)
			{
				Parent = this,
			};
		}
	}

	/// <summary>Redraws the line connecting this bone to its parent.</summary>
	private void RedrawLineToParent()
	{
		if (this.Parent == null || this.lineToParent == null)
			return;

		var endPoint = this.lineToParent.Points[1];
		endPoint.X = this.Position.OffsetX;
		endPoint.Y = this.Position.OffsetY;
		endPoint.Z = this.Position.OffsetZ;
		this.lineToParent.Points[1] = endPoint;
	}
}
