// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using Anamnesis.Memory;
using Anamnesis.Actor;

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

		if (actor.ModelObject?.Transform?.Rotation != null && actor.ModelObject?.Transform?.Position != null)
		{
			// We assume the actor is stood flat on the ground
			Vector actorEuler = actor.ModelObject.Transform.Rotation.ToEuler();
			actorEuler.X = 0;
			actorEuler.Z = 0;

			// First we use the 0 rotation position and rotate it around the actor by it's current rotation in local space
			Vector rotatedRelativePosition = Quaternion.FromEuler(actorEuler) * this.Position;

			// Adjust camera position to world space
			camService.GPoseCamera.Position = actor.ModelObject.Transform.Position + rotatedRelativePosition;

			// Now we apply the angle offset of the camera to the actor
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

		if (actor.ModelObject?.Transform?.Rotation != null && actor.ModelObject?.Transform?.Position != null)
		{
			// We assume the actor is stood flat on the ground
			Vector actorEuler = actor.ModelObject.Transform.Rotation.ToEuler();
			actorEuler.Z = 0;
			actorEuler.X = 0;

			// First we get the local coords of the current camera position in relation to the actor
			Vector localRelativePositon = camService.GPoseCamera.Position - actor.ModelObject.Transform.Position;

			// Now we calculate what the position would be if the actor had a 0 rotation
			Quaternion invertedActorRotation = Quaternion.FromEuler(actorEuler);
			invertedActorRotation.Invert();
			Vector rotatedRelativePosition = invertedActorRotation * localRelativePositon;
			this.Position = rotatedRelativePosition;

			// We save the angle of the camera as an offset from the angle of the actor
			Vector cameraEuler = camService.Camera.Euler.ToCmVector();
			this.Rotation = cameraEuler - actorEuler;
		}
	}
}
