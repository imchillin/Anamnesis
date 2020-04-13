// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows.Controls;
	using MaterialDesignColors;
	using MaterialDesignThemes.Wpf;

	/// <summary>
	/// Interaction logic for ThemeSettingsView.xaml.
	/// </summary>
	public partial class ThemeSettingsView : UserControl
	{
		private PaletteHelper paletteHelper = new PaletteHelper();
		private ThemeViewModel themeViewModel;

		public ThemeSettingsView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.themeViewModel = new ThemeViewModel();
			this.themeViewModel.Read(this.paletteHelper.GetTheme());
			this.DataContext = this.themeViewModel;
			this.themeViewModel.PropertyChanged += this.OnPropertyChanged;
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ITheme theme = this.paletteHelper.GetTheme();
			this.themeViewModel.Write(theme);
			this.paletteHelper.SetTheme(theme);
		}

		public class ThemeViewModel : INotifyPropertyChanged
		{
			public ThemeViewModel()
			{
				this.Swatches = new SwatchesProvider().Swatches;
			}

			#pragma warning disable CS0067
			public event PropertyChangedEventHandler PropertyChanged;

			public IEnumerable<Swatch> Swatches { get; }

			public bool Zodiark
			{
				get;
				set;
			}

			public double Opacity
			{
				get;
				set;
			}

			public Swatch SelectedSwatch
			{
				get;
				set;
			}

			public void Read(ITheme theme)
			{
				this.Opacity = App.Settings.Opacity;

				PaletteHelper pallete = new PaletteHelper();
				this.Zodiark = pallete.IsDark();
				this.SelectedSwatch = pallete.GetCurrentSwatch();

				if (this.SelectedSwatch == null)
				{
					this.SelectedSwatch = this.Swatches.First();
				}
			}

			public void Write(ITheme theme)
			{
				PaletteHelper pallete = new PaletteHelper();
				pallete.Apply(this.SelectedSwatch, this.Zodiark);
				App.Settings.ThemeSwatch = this.SelectedSwatch.Name;
				App.Settings.ThemeDark = this.Zodiark;
				App.Settings.Opacity = this.Opacity;
				App.Settings.Save();
			}
		}
	}
}
