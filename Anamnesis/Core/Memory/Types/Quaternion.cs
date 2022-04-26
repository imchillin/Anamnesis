// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Globalization;

public struct Quaternion : IEquatable<Quaternion>
{
	public static readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

	private static readonly float Deg2Rad = ((float)Math.PI * 2) / 360;
	private static readonly float Rad2Deg = 360 / ((float)Math.PI * 2);

	public Quaternion(float x, float y, float z, float w)
	{
		this.X = x;
		this.Y = y;
		this.Z = z;
		this.W = w;
	}

	public Quaternion(Quaternion other)
	{
		this.X = other.X;
		this.Y = other.Y;
		this.Z = other.Z;
		this.W = other.W;
	}

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	public float W { get; set; }

	public static bool operator !=(Quaternion lhs, Quaternion rhs)
	{
		return !lhs.Equals(rhs);
	}

	public static bool operator ==(Quaternion lhs, Quaternion rhs)
	{
		return lhs.Equals(rhs);
	}

	public static Quaternion operator *(Quaternion left, Quaternion right)
	{
		float x = (left.W * right.X) + (left.X * right.W) + (left.Y * right.Z) - (left.Z * right.Y);
		float y = (left.W * right.Y) + (left.Y * right.W) + (left.Z * right.X) - (left.X * right.Z);
		float z = (left.W * right.Z) + (left.Z * right.W) + (left.X * right.Y) - (left.Y * right.X);
		float w = (left.W * right.W) - (left.X * right.X) - (left.Y * right.Y) - (left.Z * right.Z);
		return new Quaternion(x, y, z, w);
	}

	public static Vector operator *(Quaternion left, Vector right)
	{
		float num = left.X * 2f;
		float num2 = left.Y * 2f;
		float num3 = left.Z * 2f;
		float num4 = left.X * num;
		float num5 = left.Y * num2;
		float num6 = left.Z * num3;
		float num7 = left.X * num2;
		float num8 = left.X * num3;
		float num9 = left.Y * num3;
		float num10 = left.W * num;
		float num11 = left.W * num2;
		float num12 = left.W * num3;
		float x = ((1f - (num5 + num6)) * right.X) + ((num7 - num12) * right.Y) + ((num8 + num11) * right.Z);
		float y = ((num7 + num12) * right.X) + ((1f - (num4 + num6)) * right.Y) + ((num9 - num10) * right.Z);
		float z = ((num8 - num11) * right.X) + ((num9 + num10) * right.Y) + ((1f - (num4 + num5)) * right.Z);
		return new Vector(x, y, z);
	}

	public static Quaternion FromString(string str)
	{
		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 4)
			throw new FormatException();

		Quaternion v = default;
		v.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		v.Z = float.Parse(parts[2], CultureInfo.InvariantCulture);
		v.W = float.Parse(parts[3], CultureInfo.InvariantCulture);
		return v;
	}

	/// <summary>
	/// Convert into a Quaternion assuming the Vector represents euler angles.
	/// </summary>
	/// <returns>Quaternion from Euler angles.</returns>
	public static Quaternion FromEuler(Vector euler)
	{
		double yaw = euler.Y * Deg2Rad;
		double pitch = euler.X * Deg2Rad;
		double roll = euler.Z * Deg2Rad;

		double c1 = Math.Cos(yaw / 2);
		double s1 = Math.Sin(yaw / 2);
		double c2 = Math.Cos(pitch / 2);
		double s2 = Math.Sin(pitch / 2);
		double c3 = Math.Cos(roll / 2);
		double s3 = Math.Sin(roll / 2);

		double c1c2 = c1 * c2;
		double s1s2 = s1 * s2;

		double x = (c1c2 * s3) + (s1s2 * c3);
		double y = (s1 * c2 * c3) + (c1 * s2 * s3);
		double z = (c1 * s2 * c3) - (s1 * c2 * s3);
		double w = (c1c2 * c3) - (s1s2 * s3);

		return new Quaternion((float)x, (float)y, (float)z, (float)w);
	}

	public bool IsApproximately(Quaternion other, float errorMargin = 0.001f)
	{
		return IsApproximately(this.X, other.X, errorMargin)
			&& IsApproximately(this.Y, other.Y, errorMargin)
			&& IsApproximately(this.Z, other.Z, errorMargin)
			&& IsApproximately(this.W, other.W, errorMargin);
	}

	public Vector ToEuler()
	{
		Vector v = default;

		double test = (this.X * this.Y) + (this.Z * this.W);

		if (test > 0.4995f)
		{
			v.Y = 2f * (float)Math.Atan2(this.X, this.Y);
			v.X = (float)Math.PI / 2;
			v.Z = 0;
		}
		else if (test < -0.4995f)
		{
			v.Y = -2f * (float)Math.Atan2(this.X, this.W);
			v.X = -(float)Math.PI / 2;
			v.Z = 0;
		}
		else
		{
			double sqx = this.X * this.X;
			double sqy = this.Y * this.Y;
			double sqz = this.Z * this.Z;

			v.Y = (float)Math.Atan2((2 * this.Y * this.W) - (2 * this.X * this.Z), 1 - (2 * sqy) - (2 * sqz));
			v.X = (float)Math.Asin(2 * test);
			v.Z = (float)Math.Atan2((2 * this.X * this.W) - (2 * this.Y * this.Z), 1 - (2 * sqx) - (2 * sqz));
		}

		v *= Rad2Deg;
		v.NormalizeAngles();

		return v;
	}

	public void Normalize()
	{
		float num = (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z) + (this.W * this.W);
		if (num > float.MaxValue)
		{
			float num2 = 1.0f / Max(Math.Abs(this.X), Math.Abs(this.Y), Math.Abs(this.Z), Math.Abs(this.W));
			this.X *= num2;
			this.Y *= num2;
			this.Z *= num2;
			this.W *= num2;
			num = (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z) + (this.W * this.W);
		}

		float num3 = 1.0f / (float)Math.Sqrt(num);
		this.X *= num3;
		this.Y *= num3;
		this.Z *= num3;
		this.W *= num3;
	}

	public override bool Equals(object? obj)
	{
		return obj is Quaternion quaternion && this.Equals(quaternion);
	}

	public bool Equals(Quaternion other)
	{
		return this.X == other.X
			&& this.Y == other.Y
			&& this.Z == other.Z
			&& this.W == other.W;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.X, this.Y, this.Z, this.W);
	}

	public void Conjugate()
	{
		this.X = 0.0f - this.X;
		this.Y = 0.0f - this.Y;
		this.Z = 0.0f - this.Z;
	}

	public void Invert()
	{
		this.Conjugate();
		float num = (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z) + (this.W * this.W);
		this.X /= num;
		this.Y /= num;
		this.Z /= num;
		this.W /= num;
	}

	public override string ToString()
	{
		return this.X.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.Y.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.Z.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.W.ToString(CultureInfo.InvariantCulture);
	}

	private static float Max(float a, float b, float c, float d)
	{
		if (b > a)
			a = b;

		if (c > a)
			a = c;

		if (d > a)
			a = d;

		return a;
	}

	private static bool IsApproximately(float a, float b, float errorMargin)
	{
		float d = MathF.Abs(a - b);
		return d < errorMargin;
	}
}
