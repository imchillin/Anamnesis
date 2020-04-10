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

	/// <summary>
	/// Interaction logic for ColorControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ColorControl : UserControl
	{
		public static readonly IBind<byte> ValueDp = Binder.Register<byte, ColorControl>(nameof(Value));
		public static readonly IBind<Appearance.Genders> GenderDp = Binder.Register<Appearance.Genders, ColorControl>(nameof(Gender));
		public static readonly IBind<ITribe> TribeDp = Binder.Register<ITribe, ColorControl>(nameof(Tribe));

		public ColorControl()
		{
			this.InitializeComponent();
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
			get
			{
				return GenderDp.Get(this);
			}
			set
			{
				GenderDp.Set(this, value);
			}
		}

		public ITribe Tribe
		{
			get
			{
				return TribeDp.Get(this);
			}

			set
			{
				TribeDp.Set(this, value);
			}
		}

		public byte Value
		{
			get
			{
				return ValueDp.Get(this);
			}
			set
			{
				ValueDp.Set(this, value);
			}
		}

		private async void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();

			FxivColorSelectorDrawer selector = new FxivColorSelectorDrawer(this.GetColors(), this.Value);
			await viewService.ShowDrawer(selector, "Color");
		}

		private ColorData.Entry[] GetColors()
		{
			switch (this.Type)
			{
				case ColorType.Skin: return ColorData.GetSkin(this.Tribe.Tribe, this.Gender);
				case ColorType.Eyes: return ColorData.GetEyeColors();
				case ColorType.Lips: return ColorData.GetLipColors();
				case ColorType.FacePaint: return ColorData.GetFacePaintColor();
				case ColorType.Hair: return ColorData.GetHair(this.Tribe.Tribe, this.Gender);
				case ColorType.HairHighlights: return ColorData.GetHairHighlights();
			}

			throw new Exception("Unsupported color type: " + this.Type);
		}
	}
}
