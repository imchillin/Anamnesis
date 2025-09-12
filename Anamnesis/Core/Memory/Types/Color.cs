// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Globalization;
using System.Runtime.CompilerServices;

/// <summary>
/// Intializes a new instance of the <see cref="Color"/> struct.
/// </summary>
/// <param name="r">The red component of the color.</param>
/// <param name="g">The green component of the color.</param>
/// <param name="b">The blue component of the color.</param>
public struct Color(float r = 1, float g = 1, float b = 1) : IEquatable<Color>
{
	/// <summary>The color white.</summary>
	public static readonly Color White = new(1, 1, 1);

	/// <summary>The color black.</summary>
	public static readonly Color Black = new(0, 0, 0);

	/// <summary>Gets or sets the red component of the color.</summary>
	public float R { get; set; } = r;

	/// <summary>Gets or sets the green component of the color.</summary>
	public float G { get; set; } = g;

	/// <summary>Gets or sets the blue component of the color.</summary>
	public float B { get; set; } = b;

	public static bool operator ==(Color a, Color b) => a.Equals(b);

	public static bool operator !=(Color a, Color b) => !a.Equals(b);

	/// <summary>Converts a string-format color to a color object.</summary>
	/// <param name="str">String representation of the color.</param>
	/// <returns>The converted color object.</returns>
	/// <exception cref="FormatException">Thrown if the string is not comprised of 3 comma separated floats.</exception>
	public static Color FromString(string str)
	{
		string[] parts = str.Split([", "], StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 3)
			throw new FormatException();

		Color v = default;
		v.R = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.G = float.Parse(parts[1], CultureInfo.InvariantCulture);
		v.B = float.Parse(parts[2], CultureInfo.InvariantCulture);
		return v;
	}

	/// <summary>
	/// Clamps a value between 0 and 1.
	/// </summary>
	/// <param name="v">The color value to clamp.</param>
	/// <returns>The clamped value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Clamp(double v) => Math.Max(0, Math.Min(v, 1));

	/// <inheritdoc/>
	public override readonly string ToString()
	{
		return this.R.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.G.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.B.ToString(CultureInfo.InvariantCulture);
	}

	/// <inheritdoc/>
	public override readonly bool Equals(object? obj) => obj is Color color && this.Equals(color);

	/// <inheritdoc/>
	public readonly bool Equals(Color other) => this.R == other.R && this.G == other.G && this.B == other.B;

	/// <inheritdoc/>
	public override readonly int GetHashCode() => HashCode.Combine(this.R, this.G, this.B);
}
