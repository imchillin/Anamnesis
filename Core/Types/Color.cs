// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.ComponentModel;

	public struct Color
	{
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
