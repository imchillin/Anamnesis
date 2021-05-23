// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System;
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for WeatherSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class WeatherSelector : UserControl, SelectorDrawer.ISelectorView
	{
		private static Modes mode = Modes.All;

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

		[Flags]
		public enum Modes
		{
			Natural = 1,
			Unnatural = 2,

			All = Natural | Unnatural,
		}

		public Modes Mode
		{
			get => mode;
			set
			{
				mode = value;
				this.Selector.FilterItems();
			}
		}

		public bool NaturalWeathers
		{
			get => this.Mode == Modes.Natural;
			set => this.Mode = Modes.Natural;
		}

		public bool UnnaturalWeathers
		{
			get => this.Mode == Modes.Unnatural;
			set => this.Mode = Modes.Unnatural;
		}

		public bool AllWeathers
		{
			get => this.Mode == Modes.All;
			set => this.Mode = Modes.All;
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
			if (obj is IWeather weather)
			{
				if (TerritoryService.Instance.CurrentTerritory != null)
				{
					bool isNatural = TerritoryService.Instance.CurrentTerritory.Weathers.Contains(weather);

					if (this.Mode == Modes.Natural && !isNatural)
					{
						return false;
					}
					else if (this.Mode == Modes.Unnatural && isNatural)
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
