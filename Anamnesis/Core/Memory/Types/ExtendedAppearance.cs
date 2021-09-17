// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
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
}
