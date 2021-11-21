// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System;
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for WeatherSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class WeatherSelector : UserControl, SelectorDrawer.ISelectorView
	{
		private static bool natrualWeathers = true;

		public WeatherSelector()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			if (TerritoryService.Instance.CurrentTerritory == null)
				return;

			this.Selector.AddItems(GameDataService.Weathers);
			this.Selector.FilterItems();
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;

		public bool NaturalWeathers
		{
			get => natrualWeathers;
			set
			{
				natrualWeathers = value;
				this.Selector.FilterItems();
			}
		}

		SelectorDrawer SelectorDrawer.ISelectorView.Selector => this.Selector;

		public void OnClosed()
		{
		}

		private void OnClose()
		{
			this.Close?.Invoke();
		}

		private void OnSelectionChanged()
		{
			this.SelectionChanged?.Invoke();
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is Weather weather)
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

			return false;
		}
	}
}
