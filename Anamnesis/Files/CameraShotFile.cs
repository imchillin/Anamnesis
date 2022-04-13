// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;

	[Serializable]
	public class CameraShotFile : JsonFileBase
	{
		public override string FileExtension => ".shot";
		public override string TypeName => "Anamnesis Camera Shot";

		public bool DelimitCamera { get; set; }
		public float Zoom { get; set; }
		public float FieldOfView { get; set; }
		public Vector2D Pan { get; set; }
		public Vector Position { get; set; }
		public Vector Rotation { get; set; }

		public void Apply(CameraService camService, ActorMemory actor)
		{
			camService.DelimitCamera = this.DelimitCamera;
			camService.Camera.Zoom = this.Zoom;
			camService.Camera.FieldOfView = this.FieldOfView;
			camService.Camera.Pan = this.Pan;

			if (actor.ModelObject?.Transform?.Position != null)
				camService.GPoseCamera.Position = this.Position + actor.ModelObject.Transform.Position;

			if (actor.ModelObject?.Transform?.Rotation != null)
			{
				Vector actorEuler = actor.ModelObject.Transform.Rotation.ToEuler();
				Vector adjusted = actorEuler + this.Rotation;
				camService.Camera.Euler = adjusted.ToMedia3DVector();
			}
		}

		public void WriteToFile(CameraService camService, ActorMemory actor)
		{
			this.DelimitCamera = camService.DelimitCamera;
			this.Zoom = camService.Camera.Zoom;
			this.FieldOfView = camService.Camera.FieldOfView;
			this.Pan = camService.Camera.Pan;

			if (actor.ModelObject?.Transform?.Position != null)
				this.Position = camService.GPoseCamera.Position - actor.ModelObject.Transform.Position;

			if (actor.ModelObject?.Transform?.Rotation != null)
			{
				Vector actorEuler = actor.ModelObject.Transform.Rotation.ToEuler();
				Vector cameraEuler = camService.Camera.Euler.ToCmVector();
				this.Rotation = cameraEuler - actorEuler;
			}
		}
	}
}
