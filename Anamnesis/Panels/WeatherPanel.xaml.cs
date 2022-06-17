// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.GameData.Excel;
using Anamnesis.Navigation;
using System.Windows;

public partial class WeatherPanel : PanelBase
{
	private readonly NavigationService.Request selectWeather;

	public WeatherPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.selectWeather = new(this, "WeatherSelector");
	}

	public TimeService TimeService => TimeService.Instance;
	public TerritoryService TerritoryService => TerritoryService.Instance;

	private void OnWeatherClicked(object sender, RoutedEventArgs e)
	{
		this.selectWeather.Navigate<Weather>((w) =>
		{
			this.TerritoryService.CurrentWeather = w;
		});
	}
}
