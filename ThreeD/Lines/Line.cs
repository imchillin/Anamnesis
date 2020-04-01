// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.ThreeD
{
	using System;
	using System.Diagnostics;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;

	public class Line : ModelVisual3D
	{
		public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(Line), new PropertyMetadata(Colors.White, OnColorChanged));
		public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(Line), new PropertyMetadata(1.0, OnThicknessChanged));
		public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(nameof(Points), typeof(Point3DCollection), typeof(Line), new PropertyMetadata(null, OnPointsChanged));

		private readonly GeometryModel3D model;
		private readonly MeshGeometry3D mesh;

		private Matrix3D visualToScreen;
		private Matrix3D screenToVisual;

		public Line()
		{
			this.mesh = new MeshGeometry3D();
			this.model = new GeometryModel3D();
			this.model.Geometry = this.mesh;
			this.SetColor(this.Color);

			this.Content = this.model;
			this.Points = new Point3DCollection();

			CompositionTarget.Rendering += this.OnRender;
		}

		public Color Color
		{
			get { return (Color)this.GetValue(ColorProperty); }
			set { this.SetValue(ColorProperty, value); }
		}

		public double Thickness
		{
			get { return (double)this.GetValue(ThicknessProperty); }
			set { this.SetValue(ThicknessProperty, value); }
		}

		public Point3DCollection Points
		{
			get { return (Point3DCollection)this.GetValue(PointsProperty); }
			set { this.SetValue(PointsProperty, value); }
		}

		public void MakeWireframe(Model3D model)
		{
			this.Points.Clear();

			if (model == null)
			{
				return;
			}

			Matrix3DStack transform = new Matrix3DStack();
			transform.Push(Matrix3D.Identity);

			this.WireframeHelper(model, transform);
		}

		public Point3D? NearestPoint2D(Point3D cameraPoint)
		{
			double closest = double.MaxValue;
			Point3D? closestPoint = null;

			Matrix3D matrix;
			if (!MathUtils.ToViewportTransform(this, out matrix))
				return null;

			MatrixTransform3D transform = new MatrixTransform3D(matrix);

			foreach (Point3D point in this.Points)
			{
				Point3D cameraSpacePoint = transform.Transform(point);

				Vector3D dir = cameraPoint - cameraSpacePoint;
				if (dir.Length < closest)
				{
					closest = dir.Length;
					closestPoint = point;
				}
			}

			return closestPoint;
		}

		private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			((Line)sender).SetColor((Color)args.NewValue);
		}

		private static void OnThicknessChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			((Line)sender).GeometryDirty();
		}

		private static void OnPointsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			((Line)sender).GeometryDirty();
		}

		private void SetColor(Color color)
		{
			MaterialGroup unlitMaterial = new MaterialGroup();
			unlitMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
			unlitMaterial.Children.Add(new EmissiveMaterial(new SolidColorBrush(color)));
			unlitMaterial.Freeze();

			this.model.Material = unlitMaterial;
			this.model.BackMaterial = unlitMaterial;
		}

		private void OnRender(object sender, EventArgs e)
		{
			if (this.Points.Count == 0 && this.mesh.Positions.Count == 0)
				return;

			if (this.UpdateTransforms())
			{
				this.RebuildGeometry();
			}
		}

		private void GeometryDirty()
		{
			// Force next call to UpdateTransforms() to return true.
			this.visualToScreen = MathUtils.ZeroMatrix;
		}

		private void RebuildGeometry()
		{
			double halfThickness = this.Thickness / 2.0;
			int numLines = this.Points.Count / 2;

			Point3DCollection positions = new Point3DCollection(numLines * 4);

			for (int i = 0; i < numLines; i++)
			{
				int startIndex = i * 2;

				Point3D startPoint = this.Points[startIndex];
				Point3D endPoint = this.Points[startIndex + 1];

				this.AddSegment(positions, startPoint, endPoint, halfThickness);
			}

			positions.Freeze();
			this.mesh.Positions = positions;

			Int32Collection indices = new Int32Collection(this.Points.Count * 3);

			for (int i = 0; i < this.Points.Count / 2; i++)
			{
				indices.Add((i * 4) + 2);
				indices.Add((i * 4) + 1);
				indices.Add((i * 4) + 0);

				indices.Add((i * 4) + 2);
				indices.Add((i * 4) + 3);
				indices.Add((i * 4) + 1);
			}

			indices.Freeze();
			this.mesh.TriangleIndices = indices;
		}

		private void AddSegment(Point3DCollection positions, Point3D startPoint, Point3D endPoint, double halfThickness)
		{
			// NOTE: We want the vector below to be perpendicular post projection so
			//       we need to compute the line direction in post-projective space.
			Vector3D lineDirection = (endPoint * this.visualToScreen) - (startPoint * this.visualToScreen);
			lineDirection.Z = 0;
			lineDirection.Normalize();

			// NOTE: Implicit Rot(90) during construction to get a perpendicular vector.
			Vector delta = new Vector(-lineDirection.Y, lineDirection.X);
			delta *= halfThickness;

			Point3D pOut1, pOut2;

			this.Widen(startPoint, delta, out pOut1, out pOut2);

			positions.Add(pOut1);
			positions.Add(pOut2);

			this.Widen(endPoint, delta, out pOut1, out pOut2);

			positions.Add(pOut1);
			positions.Add(pOut2);
		}

		private void Widen(Point3D pIn, Vector delta, out Point3D pOut1, out Point3D pOut2)
		{
			Point4D pIn4 = (Point4D)pIn;
			Point4D pOut41 = pIn4 * this.visualToScreen;
			Point4D pOut42 = pOut41;

			pOut41.X += delta.X * pOut41.W;
			pOut41.Y += delta.Y * pOut41.W;

			pOut42.X -= delta.X * pOut42.W;
			pOut42.Y -= delta.Y * pOut42.W;

			pOut41 *= this.screenToVisual;
			pOut42 *= this.screenToVisual;

			// NOTE: Z is not modified above, so we use the original Z below.
			pOut1 = new Point3D(
				pOut41.X / pOut41.W,
				pOut41.Y / pOut41.W,
				pOut41.Z / pOut41.W);

			pOut2 = new Point3D(
				pOut42.X / pOut42.W,
				pOut42.Y / pOut42.W,
				pOut42.Z / pOut42.W);
		}

		private bool UpdateTransforms()
		{
			Viewport3DVisual viewport;
			bool success;

			Matrix3D visualToScreen = MathUtils.TryTransformTo2DAncestor(this, out viewport, out success);

			if (!success || !visualToScreen.HasInverse)
			{
				this.mesh.Positions = null;
				return false;
			}

			if (visualToScreen == this.visualToScreen)
			{
				return false;
			}

			this.visualToScreen = this.screenToVisual = visualToScreen;
			this.screenToVisual.Invert();

			return true;
		}

		private void WireframeHelper(Model3D model, Matrix3DStack matrixStack)
		{
			Transform3D transform = model.Transform;

			if (transform != null && transform != Transform3D.Identity)
			{
				matrixStack.Prepend(model.Transform.Value);
			}

			try
			{
				Model3DGroup group = model as Model3DGroup;

				if (group != null)
				{
					this.WireframeHelper(group, matrixStack);
					return;
				}

				GeometryModel3D geometry = model as GeometryModel3D;

				if (geometry != null)
				{
					this.WireframeHelper(geometry, matrixStack);
					return;
				}
			}
			finally
			{
				if (transform != null && transform != Transform3D.Identity)
				{
					matrixStack.Pop();
				}
			}
		}

		private void WireframeHelper(Model3DGroup group, Matrix3DStack matrixStack)
		{
			foreach (Model3D child in group.Children)
			{
				this.WireframeHelper(child, matrixStack);
			}
		}

		private void WireframeHelper(GeometryModel3D model, Matrix3DStack matrixStack)
		{
			Geometry3D geometry = model.Geometry;
			MeshGeometry3D mesh = geometry as MeshGeometry3D;

			if (mesh != null)
			{
				Point3D[] positions = new Point3D[mesh.Positions.Count];
				mesh.Positions.CopyTo(positions, 0);
				matrixStack.Peek().Transform(positions);

				Int32Collection indices = mesh.TriangleIndices;

				if (indices.Count > 0)
				{
					int limit = positions.Length - 1;

					for (int i = 2, count = indices.Count; i < count; i += 3)
					{
						int i0 = indices[i - 2];
						int i1 = indices[i - 1];
						int i2 = indices[i];

						// WPF halts rendering on the first deformed triangle.  We should
						// do the same.
						if ((i0 < 0 || i0 > limit) || (i1 < 0 || i1 > limit) || (i2 < 0 || i2 > limit))
						{
							break;
						}

						this.AddTriangle(positions, i0, i1, i2);
					}
				}
				else
				{
					for (int i = 2, count = positions.Length; i < count; i += 3)
					{
						int i0 = i - 2;
						int i1 = i - 1;
						int i2 = i;

						this.AddTriangle(positions, i0, i1, i2);
					}
				}
			}
		}

		private void AddTriangle(Point3D[] positions, int i0, int i1, int i2)
		{
			this.Points.Add(positions[i0]);
			this.Points.Add(positions[i1]);
			this.Points.Add(positions[i1]);
			this.Points.Add(positions[i2]);
			this.Points.Add(positions[i2]);
			this.Points.Add(positions[i0]);
		}
	}
}
