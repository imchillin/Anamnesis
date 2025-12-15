// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing.Visuals;

using Anamnesis.Actor.Views;
using Anamnesis.Memory;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Media3D;
using XivToolsWpf;
using XivToolsWpf.Math3D.Extensions;

/// <summary>
/// Represents a 3D visual representation of a skeleton.
/// The visual skeleton is comprised of a collection of <see cref="BoneVisual3D"/> objects.
/// </summary>
public class SkeletonVisual3D : ModelVisual3D, IDisposable
{
	/// <summary>The root rotation of the skeleton.</summary>
	private readonly RotateTransform3D rotateTransform;
	private bool disposed = false;

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
		if (this.Skeleton.Actor.Do(a => a.ModelObject?.Transform != null) == true)
		{
			this.Skeleton.Actor.Do(a => a.ModelObject!.Transform!.PropertyChanged += this.OnTransformPropertyChanged);
		}
	}

	/// <summary>Gets the root rotation of the skeleton.</summary>
	public QuaternionRotation3D RootRotation { get; private set; }

	/// <summary>Gets the skeleton entity being visualized.</summary>
	public SkeletonEntity Skeleton { get; private set; }

	/// <summary>
	/// Disposes the resources used by the <see cref="SkeletonVisual3D"/> class.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

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

	/// <summary>
	/// Disposes the resources used by the <see cref="SkeletonVisual3D"/> class.
	/// </summary>
	/// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!this.disposed)
		{
			if (disposing)
			{
				// Dispose managed resources
				if (this.Skeleton.Actor.Do(a => a.ModelObject?.Transform != null) == true)
				{
					this.Skeleton.Actor.Do(a => a.ModelObject!.Transform!.PropertyChanged -= this.OnTransformPropertyChanged);
				}

				this.Skeleton.PropertyChanged -= this.OnSkeletonPropertyChanged;

				foreach (var child in this.Children.OfType<IDisposable>())
				{
					child.Dispose();
				}

				this.Children.Clear();
			}

			/* Dispose unmanaged resources here if any */

			this.disposed = true;
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
		if (e.PropertyName == nameof(SkeletonEntity.SelectedBones) || e.PropertyName == nameof(SkeletonEntity.HoveredBones))
		{
			await Dispatch.MainThread();

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
		if (e.PropertyName == nameof(TransformMemory.Rotation))
		{
			await Dispatch.MainThread();

			this.RootRotation.Quaternion = this.Skeleton.RootRotation.ToMedia3DQuaternion();
			this.rotateTransform.Rotation = this.RootRotation;
		}
	}
}
