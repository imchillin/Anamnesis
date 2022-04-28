// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor;

using System.Windows.Media.Media3D;

using CmQuaternion = Anamnesis.Memory.Quaternion;

public static class QuaternionExtensions
{
	public static Quaternion ToMedia3DQuaternion(this CmQuaternion self)
	{
		return new Quaternion(self.X, self.Y, self.Z, self.W);
	}

	public static CmQuaternion ToCmQuaternion(this Quaternion self)
	{
		return new CmQuaternion((float)self.X, (float)self.Y, (float)self.Z, (float)self.W);
	}

	public static void FromMedia3DQuaternion(this CmQuaternion self, Quaternion other)
	{
		self.X = (float)other.X;
		self.Y = (float)other.Y;
		self.Z = (float)other.Z;
		self.W = (float)other.W;
	}

	public static void FromCmQuaternion(this Quaternion self, CmQuaternion other)
	{
		self.X = other.X;
		self.Y = other.Y;
		self.Z = other.Z;
		self.W = other.W;
	}

	public static CmQuaternion Mirror(this CmQuaternion self)
	{
		return new CmQuaternion(self.Z, self.W, self.X, self.Y);
	}
}
