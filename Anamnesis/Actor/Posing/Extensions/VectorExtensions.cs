// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using System;
using System.Windows.Media.Media3D;

using CmVector = Anamnesis.Memory.Vector;

public static class VectorExtensions
{
	public static Vector3D ToMedia3DVector(this CmVector self)
	{
		return new Vector3D(self.X, self.Y, self.Z);
	}

	public static CmVector ToCmVector(this Vector3D self)
	{
		return new CmVector((float)self.X, (float)self.Y, (float)self.Z);
	}

	public static void FromMedia3DQuaternion(this CmVector self, Vector3D other)
	{
		self.X = (float)other.X;
		self.Y = (float)other.Y;
		self.Z = (float)other.Z;
	}

	public static void FromCmQuaternion(this Vector3D self, CmVector other)
	{
		self.X = other.X;
		self.Y = other.Y;
		self.Z = other.Z;
	}

	public static float Length(this CmVector self)
	{
		return MathF.Sqrt((self.X * self.X) + (self.Y * self.Y) + (self.Z * self.Z));
	}
}
