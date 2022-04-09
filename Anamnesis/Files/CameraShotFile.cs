// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using Anamnesis.Memory;

	[Serializable]
	public class CameraShotFile : JsonFileBase
	{
		public override string FileExtension => ".shot";
		public override string TypeName => "Anamnesis Camera Shot";

		public bool DelimitCamera { get; set; }
		public float Zoom { get; set; }
		public float FieldOfView { get; set; }
		public Vector2D Angle { get; set; }
		public Vector2D Pan { get; set; }
		public float Rotation { get; set; }
		public Vector Position { get; set; }

		public void Apply(CameraService camService)
		{
			camService.DelimitCamera = this.DelimitCamera;
			camService.Camera.Zoom = this.Zoom;
			camService.Camera.FieldOfView = this.FieldOfView;
			camService.Camera.Angle = this.Angle;
			camService.Camera.Pan = this.Pan;
			camService.Camera.Rotation = this.Rotation;
			camService.GPoseCamera.Position = this.Position;
		}

		public void WriteToFile(CameraService camService)
		{
			this.DelimitCamera = camService.DelimitCamera;
			this.Zoom = camService.Camera.Zoom;
			this.FieldOfView = camService.Camera.FieldOfView;
			this.Angle = camService.Camera.Angle;
			this.Pan = camService.Camera.Pan;
			this.Rotation = camService.Camera.Rotation;
			this.Position = camService.GPoseCamera.Position;
		}
	}
}
