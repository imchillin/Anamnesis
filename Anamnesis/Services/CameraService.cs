// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Media.Media3D;

	public class CameraService : IService
	{
		public static Quaternion Rotation;

		public Task Initialize()
		{
			////IMarshaler<Vector2D> camXY = MemoryService.GetMarshaler(Offsets.Main.CameraAddress, Offsets.Main.CameraAngle);
			////IMarshaler<float> camZ = MemoryService.GetMarshaler(Offsets.Main.CameraAddress, Offsets.Main.CameraRotation);
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		/*private void Update()
		{
			camEuler.Y = (float)MathUtils.RadiansToDegrees((double)camXY.Value.X) - 180;
			camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)camXY.Value.Y);
			camEuler.X = (float)MathUtils.RadiansToDegrees((double)camZ.Value);
			Quaternion q = camEuler.ToQuaternion();
		}*/
	}
}
