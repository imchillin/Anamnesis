// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Extensions;

using System;
using System.Windows.Media.Media3D;

public static class Matrix3DExtensions
{
	public static Quaternion ToQuaternion(this Matrix3D m)
	{
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = default;
		double tr = m.M11 + m.M22 + m.M33;

		if (tr > 0)
		{
			double s = Math.Sqrt(tr + 1.0) * 2; // S=4*qw
			q.W = 0.25 * s;
			q.X = (m.M32 - m.M23) / s;
			q.Y = (m.M13 - m.M31) / s;
			q.Z = (m.M21 - m.M12) / s;
		}
		else if ((m.M11 > m.M22) & (m.M11 > m.M33))
		{
			double s = Math.Sqrt(1.0 + m.M11 - m.M22 - m.M33) * 2; // S=4*qx
			q.W = (m.M32 - m.M23) / s;
			q.X = 0.25 * s;
			q.Y = (m.M12 + m.M21) / s;
			q.Z = (m.M13 + m.M31) / s;
		}
		else if (m.M22 > m.M33)
		{
			double s = Math.Sqrt(1.0 + m.M22 - m.M11 - m.M33) * 2; // S=4*qy
			q.W = (m.M13 - m.M31) / s;
			q.X = (m.M12 + m.M21) / s;
			q.Y = 0.25 * s;
			q.Z = (m.M23 + m.M32) / s;
		}
		else
		{
			double s = Math.Sqrt(1.0 + m.M33 - m.M11 - m.M22) * 2; // S=4*qz
			q.W = (m.M21 - m.M12) / s;
			q.X = (m.M13 + m.M31) / s;
			q.Y = (m.M23 + m.M32) / s;
			q.Z = 0.25 * s;
		}

		q.Normalize();

		return q;
	}
}
