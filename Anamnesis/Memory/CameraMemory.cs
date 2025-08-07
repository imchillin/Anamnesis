// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using PropertyChanged;
using System.Numerics;
using XivToolsWpf.Math3D;
using XivToolsWpf.Math3D.Extensions;

public class CameraMemory : MemoryBase
{
	[Bind(0x124)] public float Zoom { get; set; }
	[Bind(0x128)] public float MinZoom { get; set; }
	[Bind(0x12C)] public float MaxZoom { get; set; }
	[Bind(0x13C)] public float FieldOfView { get; set; }
	[Bind(0x140)] public Vector2 Angle { get; set; }
	[Bind(0x15C)] public float YMin { get; set; }
	[Bind(0x158)] public float YMax { get; set; }
	[Bind(0x160)] public Vector2 Pan { get; set; }
	[Bind(0x170)] public float Rotation { get; set; }

	[AlsoNotifyFor(nameof(CameraMemory.Angle), nameof(CameraMemory.Rotation))]
	public Quaternion Rotation3d
	{
		get
		{
			Vector3 camEuler = default;
			camEuler.Y = (float)MathUtils.RadiansToDegrees((double)this.Angle.X) - 180;
			camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)this.Angle.Y);
			camEuler.X = (float)MathUtils.RadiansToDegrees((double)this.Rotation);
			return QuaternionExtensions.FromEuler(camEuler);
		}
	}

	[AlsoNotifyFor(nameof(CameraMemory.Angle), nameof(CameraMemory.Rotation))]
	public Vector3 Euler
	{
		get
		{
			Vector3 camEuler = default;
			camEuler.Y = (float)MathUtils.RadiansToDegrees((double)this.Angle.X);
			camEuler.Z = (float)MathUtils.RadiansToDegrees((double)this.Angle.Y);
			camEuler.X = (float)MathUtils.RadiansToDegrees((double)this.Rotation);
			return camEuler;
		}

		set
		{
			this.Rotation = (float)MathUtils.DegreesToRadians(value.X);
			var angleX = (float)MathUtils.DegreesToRadians(value.Y);
			var angleY = (float)MathUtils.DegreesToRadians(value.Z);
			this.Angle = new Vector2(angleX, angleY);
		}
	}

	public bool FreezeAngle
	{
		get => this.IsFrozen(nameof(CameraMemory.Angle));
		set => this.SetFrozen(nameof(CameraMemory.Angle), value);
	}

	/*protected override void OnViewToModel(string fieldName, object? value)
	{
		if (!GposeService.Instance.IsGpose)
			return;

		base.OnViewToModel(fieldName, value);
	}*/
}
