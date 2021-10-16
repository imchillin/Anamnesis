// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public class ActorCustomizeMemory : MemoryBase
	{
		[Bind(0x000, BindFlags.ActorRefresh)] public Customize.Races Race { get; set; }
		[Bind(0x001, BindFlags.ActorRefresh)] public Customize.Genders Gender { get; set; }
		[Bind(0x002, BindFlags.ActorRefresh)] public Customize.Ages Age { get; set; }
		[Bind(0x003, BindFlags.ActorRefresh)] public byte Height { get; set; }
		[Bind(0x004, BindFlags.ActorRefresh)] public Customize.Tribes Tribe { get; set; }
		[Bind(0x005, BindFlags.ActorRefresh)] public byte Head { get; set; }
		[Bind(0x006, BindFlags.ActorRefresh)] public byte Hair { get; set; }
		[Bind(0x007, BindFlags.ActorRefresh)] public byte HighlightType { get; set; }
		[Bind(0x008, BindFlags.ActorRefresh)] public byte Skintone { get; set; }
		[Bind(0x009, BindFlags.ActorRefresh)] public byte REyeColor { get; set; }
		[Bind(0x00a, BindFlags.ActorRefresh)] public byte HairTone { get; set; }
		[Bind(0x00b, BindFlags.ActorRefresh)] public byte Highlights { get; set; }
		[Bind(0x00c, BindFlags.ActorRefresh)] public Customize.FacialFeature FacialFeatures { get; set; }
		[Bind(0x00d, BindFlags.ActorRefresh)] public byte FacialFeatureColor { get; set; }
		[Bind(0x00e, BindFlags.ActorRefresh)] public byte Eyebrows { get; set; }
		[Bind(0x00f, BindFlags.ActorRefresh)] public byte LEyeColor { get; set; }
		[Bind(0x010, BindFlags.ActorRefresh)] public byte Eyes { get; set; }
		[Bind(0x011, BindFlags.ActorRefresh)] public byte Nose { get; set; }
		[Bind(0x012, BindFlags.ActorRefresh)] public byte Jaw { get; set; }
		[Bind(0x013, BindFlags.ActorRefresh)] public byte Mouth { get; set; }
		[Bind(0x014, BindFlags.ActorRefresh)] public byte LipsToneFurPattern { get; set; }
		[Bind(0x015, BindFlags.ActorRefresh)] public byte EarMuscleTailSize { get; set; }
		[Bind(0x016, BindFlags.ActorRefresh)] public byte TailEarsType { get; set; }
		[Bind(0x017, BindFlags.ActorRefresh)] public byte Bust { get; set; }
		[Bind(0x018, BindFlags.ActorRefresh)] public byte FacePaint { get; set; }
		[Bind(0x019, BindFlags.ActorRefresh)] public byte FacePaintColor { get; set; }

		public bool EnableHighlights
		{
			get => this.HighlightType != 0;
			set => this.HighlightType = value ? (byte)128 : (byte)0;
		}

		public byte Lips
		{
			get => (byte)(this.EnableLipColor ? this.Mouth - 128 : this.Mouth);
			set => this.Mouth = (byte)(this.EnableLipColor ? value - 128 : value);
		}

		public bool EnableLipColor
		{
			get => this.Mouth >= 128;
			set => this.Mouth = (byte)(this.Lips + (value ? 128 : 0));
		}
	}
}
