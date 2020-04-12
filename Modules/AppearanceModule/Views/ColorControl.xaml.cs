// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Utilities;
	using ConceptMatrix.Services;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using PropertyChanged;

	using WpfColor = System.Windows.Media.Color;

	/// <summary>
	/// Interaction logic for ColorControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ColorControl : UserControl
	{
		public static readonly IBind<byte> ValueDp = Binder.Register<byte, ColorControl>(nameof(Value), OnValueChanged);
		public static readonly IBind<Appearance.Genders> GenderDp = Binder.Register<Appearance.Genders, ColorControl>(nameof(Gender), OnGenderChanged);
		public static readonly IBind<Appearance.Tribes> TribeDp = Binder.Register<Appearance.Tribes, ColorControl>(nameof(Tribe), OnTribeChanged);

		private ColorData.Entry[] colors;

		public ColorControl()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public enum ColorType
		{
			Skin,
			Eyes,
			Lips,
			FacePaint,
			Hair,
			HairHighlights,
		}

		public ColorType Type
		{
			get;
			set;
		}

		public Appearance.Genders Gender
		{
			get => GenderDp.Get(this);
			set => GenderDp.Set(this, value);
		}

		public Appearance.Tribes Tribe
		{
			get => TribeDp.Get(this);
			set => TribeDp.Set(this, value);
		}

		[AlsoNotifyFor(nameof(WpfColor))]
		public byte Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		public WpfColor WpfColor
		{
			get
			{
				if (this.colors == null)
					return System.Windows.Media.Colors.Transparent;

				return this.colors[this.Value].WpfColor;
			}
		}

		private static void OnGenderChanged(ColorControl sender, Appearance.Genders value)
		{
			sender.colors = sender.GetColors();
			sender.PreviewColor.Color = sender.WpfColor;
		}

		private static void OnTribeChanged(ColorControl sender, Appearance.Tribes value)
		{
			sender.colors = sender.GetColors();
			sender.PreviewColor.Color = sender.WpfColor;
		}

		private static void OnValueChanged(ColorControl sender, byte value)
		{
			sender.PreviewColor.Color = sender.WpfColor;
		}

		private async void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();

			FxivColorSelectorDrawer selector = new FxivColorSelectorDrawer(this.colors, this.Value);
			await viewService.ShowDrawer(selector, "Color");

			if (selector.Selected < 0 || selector.Selected >= this.colors.Length)
				return;

			this.Value = (byte)selector.Selected;
		}

		private ColorData.Entry[] GetColors()
		{
			if (this.Tribe == 0)
				return null;

			switch (this.Type)
			{
				case ColorType.Skin: return ColorData.GetSkin(this.Tribe, this.Gender);
				case ColorType.Eyes: return ColorData.GetEyeColors();
				case ColorType.Lips: return ColorData.GetLipColors();
				case ColorType.FacePaint: return ColorData.GetFacePaintColor();
				case ColorType.Hair: return ColorData.GetHair(this.Tribe, this.Gender);
				case ColorType.HairHighlights: return ColorData.GetHairHighlights();
			}

			throw new Exception("Unsupported color type: " + this.Type);
		}
	}
}
