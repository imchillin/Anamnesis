// Concept Matrix 3.
// Licensed under the MIT license.

namespace System.Windows.Media.Media3D
{
	public static class QuaternionExtensions
	{
		private static double rad2Deg = 360 / (Math.PI * 2);

		/// <summary>
		/// Converts quaternion to euler angles.
		/// </summary>
		/// <param name="q1">Quaternion to convert.</param>
		/// <returns>Vector3D as euler angles.</returns>
		public static Vector3D ToEulerAngles(this Quaternion q1)
		{
			Vector3D v = default(Vector3D);

			double test = (q1.X * q1.Y) + (q1.Z * q1.W);

			if (test > 0.4995f)
			{
				v.Y = 2f * Math.Atan2(q1.X, q1.Y);
				v.X = Math.PI / 2;
				v.Z = 0;
				return NormalizeAngles(v * rad2Deg);
			}

			if (test < -0.4995f)
			{
				v.Y = -2f * Math.Atan2(q1.X, q1.W);
				v.X = -Math.PI / 2;
				v.Z = 0;
				return NormalizeAngles(v * rad2Deg);
			}

			double sqx = q1.X * q1.X;
			double sqy = q1.Y * q1.Y;
			double sqz = q1.Z * q1.Z;

			v.Y = Math.Atan2((2 * q1.Y * q1.W) - (2 * q1.X * q1.Z), 1 - (2 * sqy) - (2 * sqz));
			v.X = Math.Asin(2 * test);
			v.Z = Math.Atan2((2 * q1.X * q1.W) - (2 * q1.Y * q1.Z), 1 - (2 * sqx) - (2 * sqz));

			return NormalizeAngles(v * rad2Deg);
		}

		private static Vector3D NormalizeAngles(Vector3D angles)
		{
			angles.X = NormalizeAngle(angles.X);
			angles.Y = NormalizeAngle(angles.Y);
			angles.Z = NormalizeAngle(angles.Z);
			return angles;
		}

		private static double NormalizeAngle(double angle)
		{
			while (angle > 360)
				angle -= 360;
			while (angle < 0)
				angle += 360;
			return angle;
		}
	}
}
