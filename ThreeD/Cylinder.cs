// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Three3D
{
	using System;
	using System.Windows.Media.Media3D;

	public class Cylinder : ModelVisual3D
	{
		private double radius;
		private GeometryModel3D model;
		private int slices = 32;
		private double length = 1;

		public Cylinder()
		{
			this.model = new GeometryModel3D();
			this.model.Geometry = this.CalculateMesh();
			this.Content = this.model;
		}

		public double Radius
		{
			get
			{
				return this.radius;
			}
			set
			{
				this.radius = value;
				this.model.Geometry = this.CalculateMesh();
			}
		}

		public int Slices
		{
			get
			{
				return this.slices;
			}
			set
			{
				this.slices = value;
				this.model.Geometry = this.CalculateMesh();
			}
		}

		public double Length
		{
			get
			{
				return this.length;
			}
			set
			{
				this.length = value;
				this.model.Geometry = this.CalculateMesh();
			}
		}

		public Material Material
		{
			get
			{
				return this.model.Material;
			}

			set
			{
				this.model.Material = value;
			}
		}

		private MeshGeometry3D CalculateMesh()
		{
			MeshGeometry3D mesh = new MeshGeometry3D();

			Vector3D axis = new Vector3D(0, this.length, 0);
			Point3D endPoint = new Point3D(0, -(this.Length / 2), 0);

			// Get two vectors perpendicular to the axis.
			Vector3D v1;
			if ((axis.Z < -0.01) || (axis.Z > 0.01))
			{
				v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
			}
			else
			{
				v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);
			}

			Vector3D v2 = Vector3D.CrossProduct(v1, axis);

			// Make the vectors have length radius.
			v1 *= this.Radius / v1.Length;
			v2 *= this.Radius / v2.Length;

			// Make the top end cap.
			// Make the end point.
			int pt0 = mesh.Positions.Count; // Index of end_point.
			mesh.Positions.Add(endPoint);

			// Make the top points.
			double theta = 0;
			double dtheta = 2 * Math.PI / this.Slices;
			for (int i = 0; i < this.Slices; i++)
			{
				mesh.Positions.Add(endPoint + (Math.Cos(theta) * v1) + (Math.Sin(theta) * v2));
				theta += dtheta;
			}

			// Make the top triangles.
			int pt1 = mesh.Positions.Count - 1; // Index of last point.
			int pt2 = pt0 + 1;                  // Index of first point.
			for (int i = 0; i < this.Slices; i++)
			{
				mesh.TriangleIndices.Add(pt0);
				mesh.TriangleIndices.Add(pt1);
				mesh.TriangleIndices.Add(pt2);
				pt1 = pt2++;
			}

			// Make the bottom end cap.
			// Make the end point.
			pt0 = mesh.Positions.Count; // Index of end_point2.
			Point3D end_point2 = endPoint + axis;
			mesh.Positions.Add(end_point2);

			// Make the bottom points.
			theta = 0;
			for (int i = 0; i < this.Slices; i++)
			{
				mesh.Positions.Add(end_point2 + (Math.Cos(theta) * v1) + (Math.Sin(theta) * v2));
				theta += dtheta;
			}

			// Make the bottom triangles.
			theta = 0;
			pt1 = mesh.Positions.Count - 1; // Index of last point.
			pt2 = pt0 + 1;                  // Index of first point.
			for (int i = 0; i < this.Slices; i++)
			{
				mesh.TriangleIndices.Add(this.Slices + 1);    // end_point2
				mesh.TriangleIndices.Add(pt2);
				mesh.TriangleIndices.Add(pt1);
				pt1 = pt2++;
			}

			// Make the sides.
			// Add the points to the mesh.
			int first_side_point = mesh.Positions.Count;
			theta = 0;
			for (int i = 0; i < this.Slices; i++)
			{
				Point3D p1 = endPoint + (Math.Cos(theta) * v1) + (Math.Sin(theta) * v2);
				mesh.Positions.Add(p1);
				Point3D p2 = p1 + axis;
				mesh.Positions.Add(p2);
				theta += dtheta;
			}

			// Make the side triangles.
			pt1 = mesh.Positions.Count - 2;
			pt2 = pt1 + 1;
			int pt3 = first_side_point;
			int pt4 = pt3 + 1;
			for (int i = 0; i < this.Slices; i++)
			{
				mesh.TriangleIndices.Add(pt1);
				mesh.TriangleIndices.Add(pt2);
				mesh.TriangleIndices.Add(pt4);

				mesh.TriangleIndices.Add(pt1);
				mesh.TriangleIndices.Add(pt4);
				mesh.TriangleIndices.Add(pt3);

				pt1 = pt3;
				pt3 += 2;
				pt2 = pt4;
				pt4 += 2;
			}

			return mesh;
		}
	}
}
