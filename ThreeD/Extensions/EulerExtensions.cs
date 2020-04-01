// Concept Matrix 3.
// Licensed under the MIT license.

namespace System.Windows.Media.Media3D
{
	public static class EulerExtensions
	{
		private static double deg2Rad = (Math.PI * 2) / 360;

		/// <summary>
		/// Convert into a Quaternion assuming the Vector3D represents euler angles.
		/// </summary>
		/// <returns>Quaternion from Euler angles.</returns>
		public static Quaternion ToQuaternion(this Vector3D self)
		{
			double yaw = self.Y * deg2Rad;
			double pitch = self.X * deg2Rad;
			double roll = self.Z * deg2Rad;

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

			return new Quaternion(x, y, z, w);
		}

		public static Vector3D NormalizeAngles(this Vector3D self)
		{
			self.X = NormalizeAngle(self.X);
			self.Y = NormalizeAngle(self.Y);
			self.Z = NormalizeAngle(self.Z);
			return self;
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
