// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System;
	using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	using Vector = Anamnesis.Vector;

	/// <summary>
	/// Interaction logic for WorldView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class HomeView : UserControl
	{
		private IGameDataService gameData;
		private IInjectionService injection;

		////private IMemory<int> timeMem;
		private IMemory<int> territoryMem;
		private IMemory<ushort> weatherMem;
		private IMemory<Vector2D> cameraAngleMem;
		private IMemory<Vector2D> cameraPanMem;
		private IMemory<float> cameraRotatonMem;
		private IMemory<float> cameraZoomMem;
		private IMemory<float> cameraFovMem;
		private IMemory<Vector> cameraPositionMem;
		private IMemory<float> cameraMinZoomMem;
		private IMemory<float> cameraMaxZoomMem;

		private int time = 0;
		private int moon = 0;
		private bool isGpose;

		public HomeView()
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
				////this.timeMem.Value = (this.moon * 86400) + (this.time * 60);
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
				////this.timeMem.Value = (this.moon * 86400) + (this.time * 60);
			}
		}

		public float CameraAngleX
		{
			get => this.CameraAngle.X;
			set
			{
				this.CameraAngle = new Vector2D(value, this.CameraAngleY);
				this.cameraAngleMem.Value = this.CameraAngle;
			}
		}

		public float CameraAngleY
		{
			get => this.CameraAngle.Y;
			set
			{
				this.CameraAngle = new Vector2D(this.CameraAngleX, value);
				this.cameraAngleMem.Value = this.CameraAngle;
			}
		}

		public bool LockCameraAngle { get; set; }
		public Vector2D CameraAngle { get; set; }
		public Vector2D CameraPan { get; set; }
		public float CameraRotaton { get; set; }
		public float CameraZoom { get; set; }
		public float CameraMinZoom { get; private set; }
		public float CameraMaxZoom { get; private set; }
		public float CameraFov { get; set; }
		public Vector CameraPosition { get; set; }

		public bool IsGpose
		{
			get
			{
				return this.isGpose;
			}

			set
			{
				this.isGpose = value;

				if (this.isGpose)
				{
					this.cameraAngleMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.CameraAngle);
					this.cameraAngleMem.ValueChanged += this.OnCameraAngleMemValueChanged;

					this.cameraPanMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.CameraPan);
					this.cameraPanMem.Bind(this, nameof(this.CameraPan));

					this.cameraRotatonMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.CameraRotation);
					this.cameraRotatonMem.Bind(this, nameof(this.CameraRotaton));

					this.cameraZoomMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.CameraCurrentZoom);
					this.cameraZoomMem.Bind(this, nameof(this.CameraZoom));

					this.cameraMinZoomMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.CameraMinZoom);
					this.cameraMinZoomMem.Bind(this, nameof(this.CameraMinZoom));

					this.cameraMaxZoomMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.CameraMaxZoom);
					this.cameraMaxZoomMem.Bind(this, nameof(this.CameraMaxZoom));

					this.cameraFovMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.FOVCurrent);
					this.cameraFovMem.Bind(this, nameof(this.CameraFov));

					this.cameraPositionMem = this.injection.GetMemory(Offsets.Main.Gpose, Offsets.Main.Camera);
					this.cameraPositionMem.Bind(this, nameof(this.CameraPosition));

					this.OnTerritoryMemValueChanged(null, 0);
				}
				else
				{
					this.cameraAngleMem.Dispose();
					this.cameraPanMem.Dispose();
					this.cameraRotatonMem.Dispose();
					this.cameraZoomMem.Dispose();
					this.cameraFovMem.Dispose();
					this.cameraPositionMem.Dispose();
					this.cameraMinZoomMem.Dispose();
					this.cameraMaxZoomMem.Dispose();
				}
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.gameData = ConceptMatrix.Services.Get<IGameDataService>();
			this.injection = ConceptMatrix.Services.Get<IInjectionService>();

			ISelectionService selectionService = ConceptMatrix.Services.Get<ISelectionService>();
			selectionService.ModeChanged += this.OnSelectionServiceModeChanged;

			////this.timeMem = this.injection.GetMemory(Offsets.Main.Time, Offsets.Main.TimeControl);
			this.weatherMem = this.injection.GetMemory(Offsets.Main.GposeFilters, Offsets.Main.ForceWeather);
			this.territoryMem = this.injection.GetMemory(Offsets.Main.TerritoryAddress, Offsets.Main.Territory);
			this.territoryMem.ValueChanged += this.OnTerritoryMemValueChanged;

			this.OnTerritoryMemValueChanged(null, 0);

			this.IsGpose = selectionService.GetMode() == Modes.GPose;
		}

		private void OnSelectionServiceModeChanged(Modes mode)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsGpose = mode == Modes.GPose;
			});
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.IsGpose = false;

			this.territoryMem.Dispose();
			this.weatherMem.Dispose();

			////this.timeMem.Value = 0;
			////this.timeMem.Dispose();
		}

		private void OnCameraAngleMemValueChanged(object sender, Vector2D value)
		{
			if (this.LockCameraAngle)
			{
				this.cameraAngleMem.Value = this.CameraAngle;
			}
			else
			{
				this.CameraAngle = this.cameraAngleMem.Value;
			}
		}

		private void OnTerritoryMemValueChanged(object sender = null, int value = 0)
		{
			int territoryId = this.territoryMem.Value;
			ushort currentWeather = this.weatherMem.Value;

			ITerritoryType territory = this.gameData.Territories.Get(territoryId);

			if (territory == null)
			{
				this.Territory = "Unknwon";

				Application.Current.Dispatcher.Invoke(() =>
				{
					this.WeatherComboBox.ItemsSource = null;
				});
			}
			else
			{
				this.Territory = territory.Region + " - " + territory.Place;

				Application.Current.Dispatcher.Invoke(() =>
				{
					this.WeatherComboBox.ItemsSource = territory.Weathers;

					foreach (IWeather weather in territory.Weathers)
					{
						byte[] bytes = { (byte)weather.Key, (byte)weather.Key };
						ushort weatherVal = BitConverter.ToUInt16(bytes, 0);

						if (weatherVal == currentWeather)
						{
							this.WeatherComboBox.SelectedItem = weather;
						}
					}
				});
			}
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

		private void OnUnlockCameraChanged(object sender, RoutedEventArgs e)
		{
			bool unlock = (bool)this.UnlockCameraCheckbox.IsChecked;

			this.CameraMaxZoom = unlock ? 256 : 20;
			this.CameraMinZoom = unlock ? 0 : 1.75f;

			using IMemory<float> minYMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.CameraYMin);
			minYMem.Value = unlock ? 1.5f : 1.25f;

			using IMemory<float> maxYMem = this.injection.GetMemory(Offsets.Main.CameraAddress, Offsets.Main.CameraYMax);
			maxYMem.Value = unlock ? -1.5f : -1.4f;
		}
	}
}
