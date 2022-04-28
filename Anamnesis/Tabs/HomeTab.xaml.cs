// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using Anamnesis.Views;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for HomeTab.xaml.
/// </summary>
public partial class HomeTab : UserControl
{
	public HomeTab()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public TipService TipService => TipService.Instance;
	public GameService GameService => GameService.Instance;
	public TargetService TargetService => TargetService.Instance;
	public GposeService GposeService => GposeService.Instance;
	public TerritoryService TerritoryService => TerritoryService.Instance;
	public TimeService TimeService => TimeService.Instance;
	public CameraService CameraService => CameraService.Instance;
	public SettingsService SettingsService => SettingsService.Instance;

	private void OnTipClicked(object sender, RoutedEventArgs e)
	{
		TipService.Instance.KnowMore();
	}

	private void OnWeatherClicked(object sender, RoutedEventArgs e)
	{
		WeatherSelector selector = new WeatherSelector();

		SelectorDrawer.Show(selector, this.TerritoryService.CurrentWeather, (w) =>
		{
			this.TerritoryService.CurrentWeather = w;
		});
	}
}
