// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.ComponentModel;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using Anamnesis.XMA.Views;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for HomeWidget.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class HomeWidget : UserControl
	{
		public HomeWidget()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			SettingsService.Current.PropertyChanged += this.OnSettingsChanged;
			this.OnSettingsChanged();
		}

		public SettingsService SettingsService => SettingsService.Instance;

		public bool ShowXMA { get; private set; }
		public bool ShowWiki { get; private set; }
		public bool ShowNone { get; private set; }

		public int Mode
		{
			get => (int)SettingsService.Current.HomeWidget;
			set => SettingsService.Current.HomeWidget = (Settings.HomeWidgetType)value;
		}

		private void OnSettingsChanged(object? sender = null, PropertyChangedEventArgs? e = null)
		{
			this.Mode = (int)SettingsService.Current.HomeWidget;
			this.ShowXMA = SettingsService.Current.HomeWidget == Settings.HomeWidgetType.XmaTop || SettingsService.Current.HomeWidget == Settings.HomeWidgetType.XmaLatest;
			this.ShowWiki = SettingsService.Current.HomeWidget == Settings.HomeWidgetType.Wiki;
			this.ShowNone = SettingsService.Current.HomeWidget == Settings.HomeWidgetType.None;
		}

		private void OnOpenUrlClicked(object sender, System.Windows.RoutedEventArgs e)
		{
			string? url = SettingsService.Current.HomeWidget switch
			{
				Settings.HomeWidgetType.XmaTop => XmaView.PopularTodaySearchUrl,
				Settings.HomeWidgetType.XmaLatest => XmaView.RecentSearchUrl,
				Settings.HomeWidgetType.Wiki => "https://github.com/imchillin/Anamnesis/wiki",
				_ => null,
			};

			if (string.IsNullOrEmpty(url))
				return;

			UrlUtility.Open(url);
		}
	}
}
