// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Globalization;

/// <summary>Represents a RGBA color.</summary>
public struct Color4 : IEquatable<Color4>
{
	/// <summary>The color white.</summary>
	public static readonly Color4 White = new(1, 1, 1, 1);

	/// <summary>The color black.</summary>
	public static readonly Color4 Black = new(0, 0, 0, 1);

	/// <summary>The color black with alpha set to 0.</summary>
	public static readonly Color4 Transparent = new(0, 0, 0, 0);

	/// <summary>The tolerance for approximate equality checks.</summary>
	private const float EQUALITY_TOLERANCE = 0.05f;

	/// <summary>
	/// Initializes a new instance of the <see cref="Color4"/> struct.
	/// </summary>
	/// <param name="r">The red component of the color.</param>
	/// <param name="g">The green component of the color.</param>
	/// <param name="b">The blue component of the color.</param>
	/// <param name="a">The alpha component of the color.</param>
	public Color4(float r = 1, float g = 1, float b = 1, float a = 1)
	{
		this.R = r;
		this.G = g;
		this.B = b;
		this.A = a;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Color4"/> struct.
	/// </summary>
	/// <param name="color">The color to copy.</param>
	/// <param name="a">The alpha component of the color.</param>
	public Color4(Color color, float a = 1)
	{
		this.R = color.R;
		this.G = color.G;
		this.B = color.B;
		this.A = a;
	}

	/// <summary>Gets or sets the red component of the color.</summary>
	public float R { get; set; }

	/// <summary>Gets or sets the green component of the color.</summary>
	public float G { get; set; }

	/// <summary>Gets or sets the blue component of the color.</summary>
	public float B { get; set; }

	/// <summary>Gets or sets the alpha component of the color.</summary>
	public float A { get; set; }

	/// <summary>Gets or sets the RGB color of the RGBA color object.</summary>
	public Color Color
	{
		readonly get
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

	public static bool operator ==(Color4 a, Color4 b) => a.Equals(b);

	public static bool operator !=(Color4 a, Color4 b) => !a.Equals(b);

	/// <summary>Converts a string-format color to a color object.</summary>
	/// <param name="str">String representation of the color.</param>
	/// <returns>The converted color object.</returns>
	/// <exception cref="FormatException">Thrown if the string is not comprised of 4 comma separated floats.</exception>
	public static Color4 FromString(string str)
	{
		if (str == null)
			return Transparent;

		string[] parts = str.Split([", "], StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 4)
			throw new FormatException();

		Color4 v = default;
		v.R = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.G = float.Parse(parts[1], CultureInfo.InvariantCulture);
		v.B = float.Parse(parts[2], CultureInfo.InvariantCulture);
		v.A = float.Parse(parts[3], CultureInfo.InvariantCulture);
		return v;
	}

	/// <inheritdoc/>
	public override readonly string ToString()
	{
		return this.R.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.G.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.B.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.A.ToString(CultureInfo.InvariantCulture);
	}

	/// <inheritdoc/>
	public override readonly bool Equals(object? obj)
	{
		return obj is Color4 color && this.Equals(color);
	}

	/// <inheritdoc/>
	public readonly bool Equals(Color4 other)
	{
		return this.R == other.R &&
			   this.G == other.G &&
			   this.B == other.B &&
			   this.A == other.A;
	}

	/// <inheritdoc/>
	public override readonly int GetHashCode() => HashCode.Combine(this.R, this.G, this.B, this.A);

	/// <summary>
	/// Checks if two colors are approximately equal.
	/// </summary>
	/// <param name="other">The color to compare to.</param>
	/// <returns>True if the colors are approximately equal, false otherwise.</returns>
	public readonly bool IsApproximately(Color4 other)
	{
		bool approx = true;
		approx &= Math.Abs(this.R - other.R) < EQUALITY_TOLERANCE;
		approx &= Math.Abs(this.G - other.G) < EQUALITY_TOLERANCE;
		approx &= Math.Abs(this.B - other.B) < EQUALITY_TOLERANCE;
		return approx;
	}
}
