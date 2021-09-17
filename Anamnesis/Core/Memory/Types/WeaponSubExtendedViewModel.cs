// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class WeaponSubExtendedViewModel : MemoryViewModelBase<WeaponExtended>
	{
		public WeaponSubExtendedViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public Vector Scale { get; set; }
		[ModelField] public Color Tint { get; set; }

		public bool IsHidden
		{
			get
			{
				return this.Scale == Vector.Zero;
			}
			set
			{
				this.Scale = value ? Vector.Zero : Vector.One;
			}
		}

		public void Hide()
		{
			this.IsHidden = true;
		}
	}
}
