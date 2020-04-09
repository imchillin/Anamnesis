// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public struct Transform
	{
		public Vector Position { get; set; }

		// We Dont know what this is for.
		public float MysteryFloat { get; set; }

		public Quaternion Rotation { get; set; }
		public Vector Scale { get; set; }
	}
}
