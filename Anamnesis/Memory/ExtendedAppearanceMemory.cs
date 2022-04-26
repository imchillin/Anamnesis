// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

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

	public bool Freeze
	{
		get => this.IsFrozen(nameof(this.SkinColor));
		set
		{
			this.SetFrozen(nameof(this.SkinColor), value);
			this.SetFrozen(nameof(this.MuscleTone), value);
			this.SetFrozen(nameof(this.MouthColor), value);
			this.SetFrozen(nameof(this.MouthColor), value);
			this.SetFrozen(nameof(this.HairColor), value);
			this.SetFrozen(nameof(this.HairGloss), value);
			this.SetFrozen(nameof(this.HairHighlight), value);
			this.SetFrozen(nameof(this.LeftEyeColor), value);
			this.SetFrozen(nameof(this.RightEyeColor), value);
			this.SetFrozen(nameof(this.LimbalRingColor), value);
		}
	}
}
