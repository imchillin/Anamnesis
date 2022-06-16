// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Styles.Drawers;
using Anamnesis.Views;
using FontAwesome.Sharp;
using System.Windows;

public partial class WeatherPanel : PanelBase
{
	public WeatherPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.Title = "Weather";
		this.Host.Icon = IconChar.Globe;
	}

	public TimeService TimeService => TimeService.Instance;
	public TerritoryService TerritoryService => TerritoryService.Instance;

	private void OnWeatherClicked(object sender, RoutedEventArgs e)
	{
		WeatherSelector selector = new WeatherSelector();

		SelectorDrawer.Show(selector, this.TerritoryService.CurrentWeather, (w) =>
		{
			this.TerritoryService.CurrentWeather = w;
		});
	}
}
