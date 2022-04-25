// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Globalization;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Vector : IEquatable<object>
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

	public static bool operator !=(Vector lhs, Vector rhs)
	{
		return !lhs.Equals(rhs);
	}

	public static bool operator ==(Vector lhs, Vector rhs)
	{
		return lhs.Equals(rhs);
	}

	public static Vector operator +(Vector left, Vector right)
	{
		return new Vector(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static Vector operator +(Vector left, float right)
	{
		return new Vector(left.X + right, left.Y + right, left.Z + right);
	}

	public static Vector operator -(Vector left, Vector right)
	{
		return new Vector(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	public static Vector operator -(Vector left, float right)
	{
		return new Vector(left.X - right, left.Y - right, left.Z - right);
	}

	public static Vector operator *(Vector left, Vector right)
	{
		return new Vector(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
	}

	public static Vector operator *(Vector left, float right)
	{
		return new Vector(left.X * right, left.Y * right, left.Z * right);
	}

	public static Vector FromString(string str)
	{
		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 3)
			throw new FormatException();

		Vector v = default;
		v.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		v.Z = float.Parse(parts[2], CultureInfo.InvariantCulture);
		return v;
	}

	public static bool IsValid(Vector? vec)
	{
		if (vec == null)
			return false;

		Vector v = (Vector)vec;
		return v.IsValid();
	}

	public void NormalizeAngles()
	{
		this.X = NormalizeAngle(this.X);
		this.Y = NormalizeAngle(this.Y);
		this.Z = NormalizeAngle(this.Z);
	}

	public override bool Equals(object? obj)
	{
		return obj is Vector quaternion && this.Equals(quaternion);
	}

	public bool Equals(Vector other)
	{
		return this.X == other.X
			&& this.Y == other.Y
			&& this.Z == other.Z;
	}

	public bool IsApproximately(Vector other, float errorMargin = 0.001f)
	{
		return IsApproximately(this.X, other.X, errorMargin)
			&& IsApproximately(this.Y, other.Y, errorMargin)
			&& IsApproximately(this.Z, other.Z, errorMargin);
	}

	public bool IsValid()
	{
		bool valid = Float.IsValid(this.X);
		valid &= Float.IsValid(this.Y);
		valid &= Float.IsValid(this.Z);

		return valid;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.X, this.Y, this.Z);
	}

	public override string ToString()
	{
		return this.X.ToString(CultureInfo.InvariantCulture) + ", " + this.Y.ToString(CultureInfo.InvariantCulture) + ", " + this.Z.ToString(CultureInfo.InvariantCulture);
	}

	private static float NormalizeAngle(float angle)
	{
		while (angle > 360)
			angle -= 360;

		while (angle < 0)
			angle += 360;

		return angle;
	}

	private static bool IsApproximately(float a, float b, float errorMargin)
	{
		float d = MathF.Abs(a - b);
		return d < errorMargin;
	}
}
