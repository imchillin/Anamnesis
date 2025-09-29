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

	public static TipService TipService => TipService.Instance;
	public static GameService GameService => GameService.Instance;
	public static TargetService TargetService => TargetService.Instance;
	public static GposeService GposeService => GposeService.Instance;
	public static TerritoryService TerritoryService => TerritoryService.Instance;
	public static TimeService TimeService => TimeService.Instance;
	public static CameraService CameraService => CameraService.Instance;
	public static SettingsService SettingsService => SettingsService.Instance;
	public static Services.Settings Settings => SettingsService.Current;

	private void OnTipClicked(object sender, RoutedEventArgs e)
	{
		TipService.Instance.KnowMore();
	}

	private void OnWeatherClicked(object sender, RoutedEventArgs e)
	{
		var selector = new WeatherSelector();
		SelectorDrawer.Show(selector, TerritoryService.CurrentWeather, (w) =>
		{
			TerritoryService.CurrentWeather = w;
		});
	}
}
