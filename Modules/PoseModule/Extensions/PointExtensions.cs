// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System.Windows.Media.Media3D;

	using CmVector = ConceptMatrix.Memory.Vector;

	public static class PointExtensions
	{
		public static Point3D ToMedia3DPoint(this CmVector self)
		{
			return new Point3D(self.X, self.Y, self.Z);
		}

		public static CmVector ToCmVector(this Point3D self)
		{
			return new CmVector((float)self.X, (float)self.Y, (float)self.Z);
		}
	}
}
