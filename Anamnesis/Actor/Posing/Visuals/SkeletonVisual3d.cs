// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing.Visuals;

using Anamnesis.Actor.Views;
using Anamnesis.Memory;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Media3D;
using XivToolsWpf;
using XivToolsWpf.Math3D.Extensions;

/// <summary>
/// Represents a 3D visual representation of a skeleton.
/// The visual skeleton is comprised of a collection of <see cref="BoneVisual3D"/> objects.
/// </summary>
public class SkeletonVisual3D : ModelVisual3D
{
	/// <summary>The root rotation of the skeleton.</summary>
	private readonly RotateTransform3D rotateTransform;

	/// <summary>
	/// Initializes a new instance of the <see cref="SkeletonVisual3D"/> class.
	/// </summary>
	/// <param name="skeleton">The skeleton entity to visualize.</param>
	public SkeletonVisual3D(SkeletonEntity skeleton)
	{
		this.Skeleton = skeleton;
		this.RootRotation = new QuaternionRotation3D(skeleton.RootRotation.ToMedia3DQuaternion());
		this.rotateTransform = new RotateTransform3D(this.RootRotation);
		this.Transform = this.rotateTransform;

		this.Initialize(skeleton);

		this.Skeleton.PropertyChanged += this.OnSkeletonPropertyChanged;
		if (this.Skeleton.Actor.ModelObject?.Transform != null)
		{
			this.Skeleton.Actor.ModelObject.Transform.PropertyChanged += this.OnTransformPropertyChanged;
		}
	}

	/// <summary>
	/// Finalizes an instance of the <see cref="SkeletonVisual3D"/> class.
	/// </summary>
	~SkeletonVisual3D()
	{
		if (this.Skeleton.Actor.ModelObject?.Transform != null)
		{
			this.Skeleton.Actor.ModelObject.Transform.PropertyChanged -= this.OnTransformPropertyChanged;
		}

		this.Skeleton.PropertyChanged -= this.OnSkeletonPropertyChanged;
	}

	/// <summary>Gets the root rotation of the skeleton.</summary>
	public QuaternionRotation3D RootRotation { get; private set; }

	/// <summary>Gets the skeleton entity being visualized.</summary>
	public SkeletonEntity Skeleton { get; private set; }

	/// <summary>Updates the visual representation of the skeleton.</summary>
	/// <remarks>
	/// Use this method if you want to update the positions and rotations
	/// of the bones in the visual tree.
	/// </remarks>
	public void Update()
	{
		foreach (BoneVisual3D bone in this.Children.OfType<BoneVisual3D>())
		{
			bone.Update();
		}
	}

	/// <summary>
	/// Updates the camera view of the skeleton.
	/// </summary>
	/// <param name="owner">The owner of the viewport.</param>
	public void OnCameraUpdated(Pose3DView owner)
	{
		foreach (BoneVisual3D bone in this.Children.OfType<BoneVisual3D>())
		{
			bone.OnCameraUpdated(owner);
		}
	}

	/// <summary>Initializes the visual representation of the skeleton.</summary>
	/// <param name="skeleton">The skeleton entity to initialize.</param>
	private void Initialize(SkeletonEntity skeleton)
	{
		this.Children.Clear();

		// Add root bones to visual tree
		foreach (var bone in skeleton.Bones.Values.OfType<BoneEntity>().Where(b => b.Parent == null))
		{
			var boneVisual = new BoneVisual3D(this, bone);
			this.Children.Add(boneVisual);
		}
	}

	/// <summary>Handles property changes in the skeleton entity.</summary>
	/// <param name="sender">The source of the property change event.</param>
	/// <param name="e">The event data.</param>
	private async void OnSkeletonPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		await Dispatch.MainThread();

		if (e.PropertyName == nameof(SkeletonEntity.SelectedBones) || e.PropertyName == nameof(SkeletonEntity.HoveredBones))
		{
			foreach (BoneVisual3D bone in this.Children.OfType<BoneVisual3D>())
			{
				bone.UpdateMaterial();
			}
		}
	}

	/// <summary>Handles the transform property changed event.</summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event arguments.</param>
	private async void OnTransformPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		await Dispatch.MainThread();

		if (e.PropertyName == nameof(TransformMemory.Rotation))
		{
			this.RootRotation.Quaternion = this.Skeleton.RootRotation.ToMedia3DQuaternion();
			this.rotateTransform.Rotation = this.RootRotation;
		}
	}
}
