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
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ThemeSettingsView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class SettingsView : UserControl
	{
		public SettingsView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.ContentArea.DataContext = this;

			this.Settings = App.Settings;
			this.Swatches = new SwatchesProvider().Swatches;

			List<double> sizes = new List<double>();
			sizes.Add(0.5);
			sizes.Add(0.6);
			sizes.Add(0.8);
			sizes.Add(0.9);
			sizes.Add(1.0);
			sizes.Add(1.25);
			sizes.Add(1.5);
			sizes.Add(1.75);
			sizes.Add(2.0);
			this.SizeSelector.ItemsSource = sizes;
		}

		public IEnumerable<Swatch> Swatches { get; }
		public MainApplicationSettings Settings { get; set; }

		public Swatch SelectedSwatch
		{
			get
			{
				foreach (Swatch sw in this.Swatches)
				{
					if (sw.Name == this.Settings.ThemeSwatch)
					{
						return sw;
					}
				}

				return this.Swatches.First();
			}

			set
			{
				this.Settings.ThemeSwatch = value.Name;
			}
		}
	}
}
