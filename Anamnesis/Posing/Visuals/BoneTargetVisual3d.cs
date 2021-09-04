// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Visuals
{
	using System;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using Anamnesis.PoseModule;
	using Anamnesis.PoseModule.Views;
	using Anamnesis.Posing.Extensions;
	using XivToolsWpf.Meida3D;

	public class BoneTargetVisual3d : ModelVisual3D
	{
		private readonly PrsTransform transform = new PrsTransform();
		private readonly PrsTransform viewportTransform = new PrsTransform();
		private readonly BoneVisual3d bone;

		public BoneTargetVisual3d(BoneVisual3d bone)
		{
			this.bone = bone;

			Viewport2DVisual3D v2d = new Viewport2DVisual3D();

			MeshGeometry3D geo = new MeshGeometry3D();
			geo.Positions.Add(new Point3D(-1, 1, 0));
			geo.Positions.Add(new Point3D(-1, -1, 0));
			geo.Positions.Add(new Point3D(1, -1, 0));
			geo.Positions.Add(new Point3D(1, 1, 0));
			geo.TextureCoordinates.Add(new Point(0, 0));
			geo.TextureCoordinates.Add(new Point(0, 1));
			geo.TextureCoordinates.Add(new Point(1, 1));
			geo.TextureCoordinates.Add(new Point(1, 0));
			geo.TriangleIndices.Add(0);
			geo.TriangleIndices.Add(1);
			geo.TriangleIndices.Add(2);
			geo.TriangleIndices.Add(0);
			geo.TriangleIndices.Add(2);
			geo.TriangleIndices.Add(3);
			v2d.Geometry = geo;

			v2d.Transform = this.viewportTransform.Transform;

			this.Transform = this.transform.Transform;

			v2d.Material = new DiffuseMaterial(new SolidColorBrush(Colors.White));

			BoneView view = new BoneView();
			view.DataContext = bone;
			v2d.Visual = view;

			Viewport2DVisual3D.SetIsVisualHostMaterial(v2d.Material, true);

			this.Children.Add(v2d);
		}

		public virtual void OnCameraUpdated(Pose3DView owner)
		{
			this.transform.Position = this.bone.WorldPosition;

			this.viewportTransform.Rotation = owner.Billboard;

			double scale = (owner.CameraDistance * 0.02) - 0.02;
			scale = Math.Clamp(scale, 0.02, 10);
			this.viewportTransform.UniformScale = scale;
		}
	}
}