// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.ThreeD
{
	using System;
	using System.Windows;
	using System.Windows.Media.Media3D;

	public class Sphere : ModelVisual3D
	{
		private GeometryModel3D model;
		private int slices = 32;
		private int stacks = 16;
		private double radius = 1;
		private Point3D center = default(Point3D);

		public Sphere()
		{
			this.model = new GeometryModel3D();
			this.model.Geometry = this.CalculateMesh();
			this.Content = this.model;
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

		public int Stacks
		{
			get
			{
				return this.stacks;
			}
			set
			{
				this.stacks = value;
				this.model.Geometry = this.CalculateMesh();
			}
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

			for (int stack = 0; stack <= this.Stacks; stack++)
			{
				double phi = (Math.PI / 2) - (stack * Math.PI / this.Stacks);
				double y = this.Radius * Math.Sin(phi);
				double scale = -this.Radius * Math.Cos(phi);

				for (int slice = 0; slice <= this.Slices; slice++)
				{
					double theta = slice * 2 * Math.PI / this.Slices;
					double x = scale * Math.Sin(theta);
					double z = scale * Math.Cos(theta);

					Vector3D normal = new Vector3D(x, y, z);
					mesh.Normals.Add(normal);
					mesh.Positions.Add(this.center + normal);
					mesh.TextureCoordinates.Add(new Point((double)slice / this.Slices, (double)stack / this.Stacks));
				}
			}

			for (int stack = 0; stack <= this.Stacks; stack++)
			{
				int top = (stack + 0) * (this.Slices + 1);
				int bot = (stack + 1) * (this.Slices + 1);

				for (int slice = 0; slice < this.Slices; slice++)
				{
					if (stack != 0)
					{
						mesh.TriangleIndices.Add(top + slice);
						mesh.TriangleIndices.Add(bot + slice);
						mesh.TriangleIndices.Add(top + slice + 1);
					}

					if (stack != this.Stacks - 1)
					{
						mesh.TriangleIndices.Add(top + slice + 1);
						mesh.TriangleIndices.Add(bot + slice);
						mesh.TriangleIndices.Add(bot + slice + 1);
					}
				}
			}

			return mesh;
		}
	}
}