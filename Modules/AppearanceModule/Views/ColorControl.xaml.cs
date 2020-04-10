// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.ComponentModel;
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
	////[AutoDependencyProperty]
	public partial class ColorControl : UserControl
	{
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
			get;
			set;
		}

		public Appearance.Tribes Tribe
		{
			get;
			set;
		}

		public byte Value
		{
			get;
			set;
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
