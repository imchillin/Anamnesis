// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows.Controls;
	using ConceptMatrix.Localization;
	using MaterialDesignColors;
	using MaterialDesignThemes.Wpf;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ThemeSettingsView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class SettingsView : UserControl
	{
		private ILocalizationService localization;

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

			List<LanguageOption> languages = new List<LanguageOption>();

			this.localization = ConceptMatrix.Services.Get<ILocalizationService>();
			foreach ((string key, string name) in this.localization.GetAvailableLocales())
			{
				languages.Add(new LanguageOption(key, name));
			}

			this.Languages = languages;
		}

		public IEnumerable<Swatch> Swatches { get; }
		public IEnumerable<LanguageOption> Languages { get; }
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

		public LanguageOption SelectedLanguage
		{
			get
			{
				foreach (LanguageOption language in this.Languages)
				{
					if (language.Key == this.Settings.Language)
					{
						return language;
					}
				}

				return this.Languages.First();
			}

			set
			{
				this.Settings.Language = value.Key;
				this.localization.SetLocale(value.Key);
			}
		}

		public class LanguageOption
		{
			public LanguageOption(string key, string display)
			{
				this.Key = key;
				this.Display = display;
			}

			public string Key { get; }
			public string Display { get; }
		}
	}
}
