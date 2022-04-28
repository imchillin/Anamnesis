// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Visuals;

using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Anamnesis.Actor;
using Anamnesis.Actor.Views;
using XivToolsWpf.Meida3D;

public class BoneTargetVisual3d : ModelVisual3D, IDisposable
{
	private readonly PrsTransform transform = new PrsTransform();
	private readonly PrsTransform sphereTransform = new PrsTransform();
	private readonly BoneVisual3d bone;

	private readonly Sphere sphere;
	private readonly Material selected;
	private readonly Material hovered;
	private readonly Material normal;

	public BoneTargetVisual3d(BoneVisual3d bone)
	{
		bone.Skeleton.PropertyChanged += this.Skeleton_PropertyChanged;

		this.bone = bone;
		this.Transform = this.transform.Transform;

		Color c = Colors.White;
		c.A = 128;
		this.normal = new DiffuseMaterial(new SolidColorBrush(c));
		this.hovered = new DiffuseMaterial(new SolidColorBrush(Colors.DarkOrange));
		this.selected = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));

		this.sphere = new Sphere();
		this.sphere.Radius = 0.015;
		this.sphere.Material = this.normal;
		this.sphere.Transform = this.sphereTransform.Transform;
		this.Children.Add(this.sphere);
	}

	public void Dispose()
	{
		this.bone.Skeleton.PropertyChanged -= this.Skeleton_PropertyChanged;
		this.Children.Clear();

		this.sphere.Children.Clear();
		this.sphere.Content = null;
	}

	public virtual void OnCameraUpdated(Pose3DView owner)
	{
		double scale = (owner.CameraDistance * 0.015) - 0.02;
		scale = Math.Clamp(scale, 0.02, 10);
		this.sphereTransform.UniformScale = scale;
	}

	private void Skeleton_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (this.bone.Skeleton.GetIsBoneHovered(this.bone))
		{
			this.sphere.Material = this.hovered;
		}
		else if (this.bone.Skeleton.GetIsBoneSelected(this.bone))
		{
			this.sphere.Material = this.selected;
		}
		else
		{
			this.sphere.Material = this.normal;
		}
	}
}
