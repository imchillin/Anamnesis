// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;

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

		public static Color FromString(string str)
		{
			string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 3)
				throw new FormatException();

			Color v = default;
			v.R = float.Parse(parts[0]);
			v.G = float.Parse(parts[0]);
			v.B = float.Parse(parts[0]);
			return v;
		}

		public override string ToString()
		{
			return this.R + ", " + this.G + ", " + this.B;
		}
	}
}
