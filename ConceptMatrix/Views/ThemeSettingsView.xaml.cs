// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
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

			public Swatch SelectedSwatch
			{
				get;
				set;
			}

			public void Read(ITheme theme)
			{
				this.Zodiark = theme.Background == Theme.Dark.MaterialDesignBackground;

				foreach (Swatch swatch in this.Swatches)
				{
					if (theme.PrimaryMid.Color == swatch.ExemplarHue.Color &&
						theme.SecondaryMid.Color == swatch.AccentExemplarHue.Color)
					{
						this.SelectedSwatch = swatch;
						break;
					}
				}

				if (this.SelectedSwatch == null)
				{
					this.SelectedSwatch = this.Swatches.First();
				}
			}

			public void Write(ITheme theme)
			{
				theme.SetBaseTheme(this.Zodiark ? Theme.Dark : Theme.Light);
				theme.SetPrimaryColor(this.SelectedSwatch.ExemplarHue.Color);

				if (this.SelectedSwatch.AccentExemplarHue != null)
				{
					theme.SetSecondaryColor(this.SelectedSwatch.AccentExemplarHue.Color);
				}
			}
		}
	}
}
