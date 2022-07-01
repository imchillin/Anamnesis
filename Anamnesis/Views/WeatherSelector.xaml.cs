// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views;

using System.Threading.Tasks;
using System.Windows.Controls;
using Anamnesis.GameData.Excel;
using Anamnesis.Services;
using XivToolsWpf;

public partial class WeatherSelector : UserControl
{
	private static bool natrualWeathers = true;

	public WeatherSelector()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public bool NaturalWeathers
	{
		get => natrualWeathers;
		set
		{
			natrualWeathers = value;
			this.Selector.FilterItems();
		}
	}

	protected Task LoadItems()
	{
		if (TerritoryService.Instance.CurrentTerritory == null)
			return Task.CompletedTask;

		this.Selector.AddItems(GameDataService.Weathers);

		return Task.CompletedTask;
	}

	protected bool Filter(Weather weather, string[]? search)
	{
		if (weather.RowId == 0)
			return false;

		if (TerritoryService.Instance.CurrentTerritory != null)
		{
			bool isNatural = TerritoryService.Instance.CurrentTerritory.Weathers.Contains(weather);

			if (natrualWeathers && !isNatural)
			{
				return false;
			}
		}

		bool matches = SearchUtility.Matches(weather.Name, search);
		matches |= SearchUtility.Matches(weather.Description, search);
		matches |= SearchUtility.Matches(weather.WeatherId.ToString(), search);

		if (!matches)
			return false;

		return true;
	}
}
