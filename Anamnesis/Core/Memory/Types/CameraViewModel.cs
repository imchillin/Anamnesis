// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Types
{
	using System;
	using System.Windows.Media.Media3D;
	using PropertyChanged;
	using XivToolsWpf.Meida3D;

	[AddINotifyPropertyChangedInterface]
	public class CameraViewModel : MemoryViewModelBase<Camera>
	{
		public CameraViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public Vector2D Angle { get; set; }
		[ModelField] public float YMin { get; set; }
		[ModelField] public float YMax { get; set; }
		[ModelField] public Vector2D Pan { get; set; }
		[ModelField] public float Rotation { get; set; }
		[ModelField] public float Zoom { get; set; }
		[ModelField] public float MinZoom { get; set; }
		[ModelField] public float MaxZoom { get; set; }
		[ModelField] public float FieldOfView { get; set; }

		[AlsoNotifyFor(nameof(CameraViewModel.Angle), nameof(CameraViewModel.Rotation))]
		public Quaternion Rotation3d
		{
			get
			{
				Vector3D camEuler = default;
				camEuler.Y = (float)MathUtils.RadiansToDegrees((double)this.Angle.X) - 180;
				camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)this.Angle.Y);
				camEuler.X = (float)MathUtils.RadiansToDegrees((double)this.Rotation);
				return camEuler.ToQuaternion();
			}
		}

		public bool FreezeAngle
		{
			get => this.IsValueFrozen(nameof(CameraViewModel.Angle));
			set => this.FreezeValue(nameof(CameraViewModel.Angle), value);
		}

		/*protected override void OnViewToModel(string fieldName, object? value)
		{
			if (!GposeService.Instance.IsGpose)
				return;

			base.OnViewToModel(fieldName, value);
		}*/
	}
}
