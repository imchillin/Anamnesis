// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Globalization;

public struct Vector2D
{
	public static readonly Vector2D Zero = new Vector2D(0, 0);
	public static readonly Vector2D One = new Vector2D(1, 1);

	public Vector2D(float x = 0, float y = 0)
	{
		this.X = x;
		this.Y = y;
	}

	public float X { get; set; }
	public float Y { get; set; }

	public static bool operator !=(Vector2D lhs, Vector2D rhs)
	{
		return !lhs.Equals(rhs);
	}

	public static bool operator ==(Vector2D lhs, Vector2D rhs)
	{
		return lhs.Equals(rhs);
	}

	public static Vector2D operator +(Vector2D left, Vector2D right)
	{
		return new Vector2D(left.X + right.X, left.Y + right.Y);
	}

	public static Vector2D operator -(Vector2D left, Vector2D right)
	{
		return new Vector2D(left.X - right.X, left.Y - right.Y);
	}

	public static Vector2D operator *(Vector2D left, Vector2D right)
	{
		return new Vector2D(left.X * right.X, left.Y * right.Y);
	}

	public static Vector2D operator *(Vector2D left, float right)
	{
		return new Vector2D(left.X * right, left.Y * right);
	}

	public static Vector2D FromString(string str)
	{
		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 3)
			throw new FormatException();

		Vector2D v = default;
		v.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		return v;
	}

	public override bool Equals(object? obj)
	{
		return obj is Vector2D quaternion && this.Equals(quaternion);
	}

	public bool Equals(Vector2D other)
	{
		return this.X == other.X
			&& this.Y == other.Y;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.X, this.Y);
	}

	public override string ToString()
	{
		return this.X.ToString(CultureInfo.InvariantCulture) + ", " + this.Y.ToString(CultureInfo.InvariantCulture);
	}
}
