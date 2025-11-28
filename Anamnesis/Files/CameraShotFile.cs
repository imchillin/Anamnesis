// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Memory;
using System;
using System.Numerics;
using XivToolsWpf.Math3D.Extensions;

[Serializable]
public class CameraShotFile : JsonFileBase
{
	public override string FileExtension => ".shot";
	public override string TypeName => "Anamnesis Camera Shot";

	public bool DelimitCamera { get; set; }
	public float Zoom { get; set; }
	public float FieldOfView { get; set; }
	public Vector2 Pan { get; set; }
	public Vector3 Position { get; set; }
	public Vector3 Rotation { get; set; }

	public void Apply(CameraService camService, ObjectHandle<GameObjectMemory> handle)
	{
		handle.Do(actor => this.Apply(camService, actor));
	}

	public void WriteToFile(CameraService camService, ObjectHandle<GameObjectMemory> handle)
	{
		handle.Do(actor => this.WriteToFile(camService, actor));
	}

	private void Apply(CameraService camService, GameObjectMemory actor)
	{
		camService.DelimitCamera = this.DelimitCamera;
		camService.Camera.Zoom = this.Zoom;
		camService.Camera.FieldOfView = this.FieldOfView;
		camService.Camera.Pan = this.Pan;

		if (actor.ModelObject?.Transform?.Rotation != null && actor.ModelObject?.Transform?.Position != null)
		{
			// We assume the actor is stood flat on the ground
			Vector3 actorEuler = actor.ModelObject.Transform.Rotation.ToEuler();
			actorEuler.X = 0;
			actorEuler.Z = 0;

			// First we use the 0 rotation position and rotate it around the actor by it's current rotation in local space
			Vector3 rotatedRelativePosition = Vector3.Transform(this.Position, QuaternionExtensions.FromEuler(actorEuler));

			// Adjust camera position to world space
			camService.GPoseCamera.Position = actor.ModelObject.Transform.Position + rotatedRelativePosition;

			// Now we apply the angle offset of the camera to the actor
			camService.Camera.Euler = actorEuler + this.Rotation;
		}
	}

	private void WriteToFile(CameraService camService, GameObjectMemory actor)
	{
		this.DelimitCamera = camService.DelimitCamera;
		this.Zoom = camService.Camera.Zoom;
		this.FieldOfView = camService.Camera.FieldOfView;
		this.Pan = camService.Camera.Pan;

		if (actor.ModelObject?.Transform?.Rotation != null && actor.ModelObject?.Transform?.Position != null)
		{
			// We assume the actor is stood flat on the ground
			Vector3 actorEuler = actor.ModelObject.Transform.Rotation.ToEuler();
			actorEuler.Z = 0;
			actorEuler.X = 0;

			// First we get the local coords of the current camera position in relation to the actor
			Vector3 localRelativePositon = camService.GPoseCamera.Position - actor.ModelObject.Transform.Position;

			// Now we calculate what the position would be if the actor had a 0 rotation
			Quaternion invertedActorRotation = Quaternion.Inverse(QuaternionExtensions.FromEuler(actorEuler));
			Vector3 rotatedRelativePosition = Vector3.Transform(localRelativePositon, invertedActorRotation);
			this.Position = rotatedRelativePosition;

			// We save the angle of the camera as an offset from the angle of the actor
			Vector3 cameraEuler = camService.Camera.Euler;
			this.Rotation = cameraEuler - actorEuler;
		}
	}
}
