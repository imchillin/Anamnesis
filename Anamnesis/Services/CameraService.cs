// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Media.Media3D;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;

	using MediaQuaternion = System.Windows.Media.Media3D.Quaternion;

	public class CameraService : ServiceBase<CameraService>
	{
		public static MediaQuaternion Rotation;

		public bool LockCameraAngle { get; set; }
		public Vector2D CameraAngle { get; set; }
		public Vector2D CameraPan { get; set; }
		public Vector CameraPosition { get; set; }
		public bool LockCameraPosition { get; set; }
		public float CameraRotaton { get; set; }
		public float CameraZoom { get; set; }
		public float CameraMinZoom { get; private set; }
		public float CameraMaxZoom { get; private set; }
		public float CameraFov { get; set; }

		public override async Task Start()
		{
			await base.Start();
			_ = Task.Run(this.WatchCameraThread);
		}

		private async Task WatchCameraThread()
		{
			while (this.IsAlive)
			{
				IntPtr ptr = MemoryService.ReadPtr(AddressService.Camera);
				this.CameraAngle = MemoryService.Read<Vector2D>(ptr + 0x130);
				this.CameraPan = MemoryService.Read<Vector2D>(ptr + 0x150);
				this.CameraRotaton = MemoryService.Read<float>(ptr + 0x160);
				this.CameraZoom = MemoryService.Read<float>(ptr + 0x114);
				this.CameraMinZoom = MemoryService.Read<float>(ptr + 0x118);
				this.CameraMaxZoom = MemoryService.Read<float>(ptr + 0x11C);
				this.CameraFov = MemoryService.Read<float>(ptr + 0x12C);

				IntPtr gposePtr = MemoryService.ReadPtr(AddressService.GPose);
				this.CameraPosition = MemoryService.Read<Vector>(gposePtr + 0xA0);

				/*camEuler.Y = (float)MathUtils.RadiansToDegrees((double)camXY.Value.X) - 180;
				camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)camXY.Value.Y);
				camEuler.X = (float)MathUtils.RadiansToDegrees((double)camZ.Value);
				Rotation = camEuler.ToQuaternion();*/

				// maximum 60 times a second
				await Task.Delay(16);
			}
		}
	}
}
