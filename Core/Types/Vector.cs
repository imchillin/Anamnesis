// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public struct Vector
	{
		public static readonly Vector Zero = new Vector(0, 0, 0);
		public static readonly Vector One = new Vector(1, 1, 1);

		public Vector(float x = 0, float y = 0, float z = 0)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
	}
}
