// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct ExtendedAppearance
	{
		[FieldOffset(0x00)] public Color SkinColor;
		[FieldOffset(0x0C)] public float MuscleTone;
		[FieldOffset(0x10)] public Color SkinGloss;
		[FieldOffset(0x20)] public Color4 MouthColor;
		[FieldOffset(0x30)] public Color HairColor;
		[FieldOffset(0x40)] public Color HairGloss;
		[FieldOffset(0x50)] public Color HairHighlight;
		[FieldOffset(0x60)] public Color LeftEyeColor;
		[FieldOffset(0x70)] public Color RightEyeColor;
		[FieldOffset(0x80)] public Color LimbalRingColor;
	}

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
