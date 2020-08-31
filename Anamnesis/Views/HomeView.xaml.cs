// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System;
	using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using PropertyChanged;

	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for WorldView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class HomeView : UserControl
	{
		public HomeView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public string Territory { get; set; } = "Unknown";

		public float CameraAngleX
		{
			get => this.CameraAngle.X;
			set
			{
				this.CameraAngle = new Vector2D(value, this.CameraAngleY);
				////this.cameraAngleMem?.SetValue(this.CameraAngle);
			}
		}

		public float CameraAngleY
		{
			get => this.CameraAngle.Y;
			set
			{
				this.CameraAngle = new Vector2D(this.CameraAngleX, value);
				////this.cameraAngleMem?.SetValue(this.CameraAngle);
			}
		}

		public bool LockCameraAngle { get; set; }
		public Vector2D CameraAngle { get; set; }
		public Vector2D CameraPan { get; set; }
		public Vector CameraPosition { get; set; }
		public bool LockCameraPosition { get; set; }
		public float CameraRotaton { get; set; }
		public float CameraZoom { get; set; }
		public float CameraMinZoom { get; private set; }
		public float CameraMaxZoom { get; private set; }
		public float CameraFov { get; set; }
		public Vector Position { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector Scale { get; set; }

		public bool IsGpose { get; set; }

		public ActorViewModel? Target { get; set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			TargetService.ModeChanged += this.OnTargetModeChanged;
			this.IsGpose = true;

			this.SetActor(this.DataContext as ActorViewModel);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.SetActor(this.DataContext as ActorViewModel);
		}

		private void OnTargetModeChanged(Modes mode)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsGpose = mode == Modes.GPose;
			});
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.IsGpose = false;

			this.SetActor(null);

			////this.territoryMem?.Dispose();
			////this.weatherMem?.Dispose();
		}

		/*private void OnCameraAngleMemValueChanged(object? sender, Vector2D value)
		{
			if (this.LockCameraAngle)
			{
				this.cameraAngleMem?.SetValue(this.CameraAngle);
			}
			else if (this.cameraAngleMem != null)
			{
				this.CameraAngle = this.cameraAngleMem.Value;
			}
		}

		private void OnTerritoryMemValueChanged(object? sender = null, int value = 0)
		{
			if (this.territoryMem == null || this.weatherMem == null)
				return;

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
		}*/

		private void OnWeatherSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			IWeather? weather = this.WeatherComboBox.SelectedItem as IWeather;

			if (weather == null)
				return;

			// This is super weird. I have no idea why we need to do this for weather...
			////byte[] bytes = { (byte)weather.Key, (byte)weather.Key };
			////this.weatherMem?.SetValue(BitConverter.ToUInt16(bytes, 0));
		}

		private void OnUnlockCameraChanged(object sender, RoutedEventArgs e)
		{
			if (this.UnlockCameraCheckbox.IsChecked == null)
				this.UnlockCameraCheckbox.IsChecked = false;

			bool unlock = (bool)this.UnlockCameraCheckbox.IsChecked;

			this.CameraMaxZoom = unlock ? 256 : 20;
			this.CameraMinZoom = unlock ? 0 : 1.75f;

			////using IMarshaler<float> minYMem = MemoryService.GetMarshaler(Offsets.Main.CameraAddress, Offsets.Main.CameraYMin);
			////minYMem.Value = unlock ? 1.5f : 1.25f;

			////using IMarshaler<float> maxYMem = MemoryService.GetMarshaler(Offsets.Main.CameraAddress, Offsets.Main.CameraYMax);
			////maxYMem.Value = unlock ? -1.5f : -1.4f;
		}

		private void SetActor(ActorViewModel? actor)
		{
			this.Target = actor;

			/*this.cameraPositionMem?.Dispose();
			this.posMem?.Dispose();
			this.rotMem?.Dispose();
			this.scaleMem?.Dispose();

			if (actor == null)
				return;

			if (!this.initialized)
			{
				this.initialized = true;

				this.weatherMem = MemoryService.GetMarshaler(Offsets.Main.GposeFilters, Offsets.Main.ForceWeather);
				this.territoryMem = null;
				this.territoryMem = MemoryService.GetMarshaler(Offsets.Main.TerritoryAddress, Offsets.Main.Territory);
				this.territoryMem.ValueChanged += this.OnTerritoryMemValueChanged;
				this.OnTerritoryMemValueChanged(null, 0);

				this.IsGpose = TargetService.CurrentMode == Modes.GPose;
			}

			this.posMem = actor.GetMemory(Offsets.Main.Position);
			this.posMem.Bind(this, nameof(this.Position));

			this.rotMem = actor.GetMemory(Offsets.Main.Rotation);
			this.rotMem.Bind(this, nameof(this.Rotation));

			this.scaleMem = actor.GetMemory(Offsets.Main.Scale);
			this.scaleMem.Bind(this, nameof(this.Scale));

			if (this.isGpose)
			{
				this.cameraPositionMem = MemoryService.GetMarshaler(Offsets.Main.Gpose, Offsets.Main.Camera);

				if (this.LockCameraPosition)
				{
					this.cameraPositionMem.Value = this.CameraPosition;
				}
				else
				{
					this.cameraPositionMem.Value = actor.GetValue(Offsets.Main.Position);
				}

				this.cameraPositionMem.Bind(this, nameof(this.CameraPosition));
			}*/
		}
	}
}
