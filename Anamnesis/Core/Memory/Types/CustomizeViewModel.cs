// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class CustomizeViewModel : StructViewModelBase<Customize>
	{
		public CustomizeViewModel(ActorViewModel parent)
			: base(parent, nameof(ActorViewModel.Customize))
		{
		}

		public CustomizeViewModel(ActorViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public Customize.Races Race { get; set; }
		[ModelField] public Customize.Genders Gender { get; set; }
		[ModelField] public Customize.Ages Age { get; set; }
		[ModelField] public byte Height { get; set; }
		[ModelField] public Customize.Tribes Tribe { get; set; }
		[ModelField] public byte Head { get; set; }
		[ModelField] public byte Hair { get; set; }
		[ModelField] public byte HighlightType { get; set; }
		[ModelField] public byte Skintone { get; set; }
		[ModelField] public byte REyeColor { get; set; }
		[ModelField] public byte HairTone { get; set; }
		[ModelField] public byte Highlights { get; set; }
		[ModelField] public Customize.FacialFeature FacialFeatures { get; set; }
		[ModelField] public byte FacialFeatureColor { get; set; }
		[ModelField] public byte Eyebrows { get; set; }
		[ModelField] public byte LEyeColor { get; set; }
		[ModelField] public byte Eyes { get; set; }
		[ModelField] public byte Nose { get; set; }
		[ModelField] public byte Jaw { get; set; }
		[ModelField] public byte Mouth { get; set; }
		[ModelField] public byte LipsToneFurPattern { get; set; }
		[ModelField] public byte EarMuscleTailSize { get; set; }
		[ModelField] public byte TailEarsType { get; set; }
		[ModelField] public byte Bust { get; set; }
		[ModelField] public byte FacePaint { get; set; }
		[ModelField] public byte FacePaintColor { get; set; }

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
