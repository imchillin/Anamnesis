// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public struct Color
	{
		public static readonly Color White = new Color(1, 1, 1);
		public static readonly Color Black = new Color(0, 0, 0);

		public Color(float r = 1, float g = 1, float b = 1)
		{
			this.R = r;
			this.G = g;
			this.B = b;
		}

		public float R { get; set; }
		public float G { get; set; }
		public float B { get; set; }
	}
}
