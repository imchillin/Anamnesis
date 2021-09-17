// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ExtendedAppearanceViewModel : MemoryViewModelBase<ExtendedAppearance>
	{
		public ExtendedAppearanceViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public Color SkinColor { get; set; }
		[ModelField] public float MuscleTone { get; set; }
		[ModelField] public Color SkinGloss { get; set; }
		[ModelField] public Color4 MouthColor { get; set; }
		[ModelField] public Color HairColor { get; set; }
		[ModelField] public Color HairGloss { get; set; }
		[ModelField] public Color HairHighlight { get; set; }
		[ModelField] public Color LeftEyeColor { get; set; }
		[ModelField] public Color RightEyeColor { get; set; }
		[ModelField] public Color LimbalRingColor { get; set; }
	}
}
