// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class ExtendedAppearanceMemory : MemoryBase
	{
		[Bind(0x00)] public Color SkinColor { get; set; }
		[Bind(0x0C)] public float MuscleTone { get; set; }
		[Bind(0x10)] public Color SkinGloss { get; set; }
		[Bind(0x20)] public Color4 MouthColor { get; set; }
		[Bind(0x30)] public Color HairColor { get; set; }
		[Bind(0x40)] public Color HairGloss { get; set; }
		[Bind(0x50)] public Color HairHighlight { get; set; }
		[Bind(0x60)] public Color LeftEyeColor { get; set; }
		[Bind(0x70)] public Color RightEyeColor { get; set; }
		[Bind(0x80)] public Color LimbalRingColor { get; set; }
	}
}
