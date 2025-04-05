// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views;

using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using System.Linq;
using System.Threading.Tasks;
using XivToolsWpf;

public abstract class WeatherSelectorDrawer : SelectorDrawer<Weather>
{
}

/// <summary>
/// Interaction logic for WeatherSelector.xaml.
/// </summary>
public partial class WeatherSelector : WeatherSelectorDrawer
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
			this.FilterItems();
		}
	}

	protected override Task LoadItems()
	{
		if (TerritoryService.Instance.CurrentTerritory == null)
			return Task.CompletedTask;

		this.AddItems(GameDataService.Weathers.ToEnumerable());

		return Task.CompletedTask;
	}

	protected override bool Filter(Weather weather, string[]? search)
	{
		if (weather.RowId == 0)
			return false;

		if (TerritoryService.Instance.CurrentTerritory != null)
		{
			bool isNatural = TerritoryService.Instance.CurrentTerritory.Value.WeatherRate.Value.Weather.Any(w => w.RowId == weather.RowId);

			if (natrualWeathers && !isNatural)
			{
				return false;
			}
		}

		bool matches = SearchUtility.Matches(weather.Name, search);
		matches |= SearchUtility.Matches(weather.Description, search);
		matches |= SearchUtility.Matches(weather.RowId.ToString(), search);

		if (!matches)
			return false;

		return true;
	}
}
