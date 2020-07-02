// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System;
	using System.Collections.ObjectModel;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for WorldView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class WorldView : UserControl
	{
		private IGameDataService gameData;

		private IMemory<int> timeMem;
		private IMemory<int> territoryMem;
		private IMemory<ushort> weatherMem;

		private int time = 0;
		private int moon = 0;

		public WorldView()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public string Territory { get; set; }

		public int Time
		{
			get
			{
				return this.time;
			}

			set
			{
				this.time = value;
				this.timeMem.Value = (this.moon * 86400) + (this.time * 60);
			}
		}

		public int Moon
		{
			get
			{
				return this.moon;
			}

			set
			{
				this.moon = value;
				this.timeMem.Value = (this.moon * 86400) + (this.time * 60);
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.gameData = ConceptMatrix.Services.Get<IGameDataService>();
			IInjectionService injection = ConceptMatrix.Services.Get<IInjectionService>();

			this.timeMem = injection.GetMemory(Offsets.Main.Time, Offsets.Main.TimeControl);

			this.territoryMem = injection.GetMemory(Offsets.Main.TerritoryAddress, Offsets.Main.Territory);
			this.territoryMem.ValueChanged += this.OnTerritoryMemValueChanged;

			this.weatherMem = injection.GetMemory(Offsets.Main.GposeFilters, Offsets.Main.ForceWeather);

			this.OnTerritoryMemValueChanged(null, null);
		}

		private void OnTerritoryMemValueChanged(object sender = null, object value = null)
		{
			int territoryId = this.territoryMem.Value;

			foreach (ITerritoryType territory in this.gameData.Territories.All)
			{
				if (territory.Key == territoryId)
				{
					this.Territory = territory.Region + " - " + territory.Place;

					this.WeatherComboBox.ItemsSource = territory.Weathers;
				}
			}
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.timeMem.Dispose();
			this.territoryMem.Dispose();
		}

		private void OnWeatherSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			IWeather weather = this.WeatherComboBox.SelectedItem as IWeather;

			if (weather == null)
				return;

			// This is super weird. I have no idea why we need to do this for weather...
			byte[] bytes = { (byte)weather.Key, (byte)weather.Key };
			this.weatherMem.Value = BitConverter.ToUInt16(bytes, 0);
		}
	}
}
