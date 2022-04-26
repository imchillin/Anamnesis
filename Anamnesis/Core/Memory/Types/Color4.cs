// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Globalization;

public struct Color4 : IEquatable<Color4>
{
	public static readonly Color4 White = new Color4(1, 1, 1, 1);
	public static readonly Color4 Black = new Color4(0, 0, 0, 1);
	public static readonly Color4 Transparent = new Color4(0, 0, 0, 0);

	public Color4(float r = 1, float g = 1, float b = 1, float a = 1)
	{
		this.R = r;
		this.G = g;
		this.B = b;
		this.A = a;
	}

	public Color4(Color color, float a = 1)
	{
		this.R = color.R;
		this.G = color.G;
		this.B = color.B;
		this.A = a;
	}

	public float R { get; set; }
	public float G { get; set; }
	public float B { get; set; }
	public float A { get; set; }

	public Color Color
	{
		get
		{
			return new Color(this.R, this.G, this.B);
		}

		set
		{
			this.R = value.R;
			this.G = value.G;
			this.B = value.B;
		}
	}

	public static bool operator ==(Color4 a, Color4 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Color4 a, Color4 b)
	{
		return !a.Equals(b);
	}

	public static Color4 FromString(string str)
	{
		if (str == null)
			return Color4.Transparent;

		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 4)
			throw new FormatException();

		Color4 v = default;
		v.R = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.G = float.Parse(parts[1], CultureInfo.InvariantCulture);
		v.B = float.Parse(parts[2], CultureInfo.InvariantCulture);
		v.A = float.Parse(parts[3], CultureInfo.InvariantCulture);
		return v;
	}

	public override string ToString()
	{
		return this.R.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.G.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.B.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.A.ToString(CultureInfo.InvariantCulture);
	}

	public override bool Equals(object? obj)
	{
		return obj is Color4 color && this.Equals(color);
	}

	public bool Equals(Color4 other)
	{
		return this.R == other.R &&
			   this.G == other.G &&
			   this.B == other.B &&
			   this.A == other.A;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.R, this.G, this.B, this.A);
	}

	public bool IsApproximately(Color4 other)
	{
		bool approx = true;
		approx &= Math.Abs(this.R - other.R) < 0.05f;
		approx &= Math.Abs(this.G - other.G) < 0.05f;
		approx &= Math.Abs(this.B - other.B) < 0.05f;
		return approx;
	}
}
