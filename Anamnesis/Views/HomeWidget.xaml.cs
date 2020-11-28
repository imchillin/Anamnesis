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
		public bool ShowArt { get; private set; }

		public int Mode
		{
			get => (int)SettingsService.Current.HomeWidget;
			set => SettingsService.Current.HomeWidget = (Settings.HomeWidgetType)value;
		}

		private void OnSettingsChanged(object? sender = null, PropertyChangedEventArgs? e = null)
		{
			this.Mode = (int)SettingsService.Current.HomeWidget;
			this.ShowXMA = SettingsService.Current.HomeWidget == Settings.HomeWidgetType.XmaTop || SettingsService.Current.HomeWidget == Settings.HomeWidgetType.XmaLatest;
			this.ShowArt = SettingsService.Current.HomeWidget == Settings.HomeWidgetType.Art;
		}

		private void OnOpenUrlClicked(object sender, System.Windows.RoutedEventArgs e)
		{
			string? url = SettingsService.Current.HomeWidget switch
			{
				Settings.HomeWidgetType.XmaTop => XmaView.PopularTodaySearchUrl,
				Settings.HomeWidgetType.XmaLatest => XmaView.RecentSearchUrl,
				Settings.HomeWidgetType.Art => "https://discord.com/channels/701987194910277642/782161117463969793/782165438666637343",
				_ => null,
			};

			if (string.IsNullOrEmpty(url))
				return;

			UrlUtility.Open(url);
		}
	}
}
