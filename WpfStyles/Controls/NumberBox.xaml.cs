// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System;
	using System.ComponentModel;
	using System.Data;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using ConceptMatrix.WpfStyles.DependencyProperties;

	/// <summary>
	/// Interaction logic for NumberBox.xaml.
	/// </summary>
	public partial class NumberBox : UserControl, INotifyPropertyChanged
	{
		public static readonly IBind<double> ValueDp = Binder.Register<double, NumberBox>(nameof(Value), OnValueChanged);
		public static readonly IBind<double> TickDp = Binder.Register<double, NumberBox>(nameof(TickFrequency));
		public static readonly IBind<bool> SliderDp = Binder.Register<bool, NumberBox>(nameof(Slider));
		public static readonly IBind<double> MinDp = Binder.Register<double, NumberBox>(nameof(Minimum));
		public static readonly IBind<double> MaxDp = Binder.Register<double, NumberBox>(nameof(Maximum));
		public static readonly IBind<bool> WrapDp = Binder.Register<bool, NumberBox>(nameof(Wrap));

		private string inputString;
		private Key keyHeld = Key.None;

		public NumberBox()
		{
			this.InitializeComponent();
			this.TickFrequency = 1;
			this.ContentArea.DataContext = this;

			this.Minimum = double.MinValue;
			this.Maximum = double.MaxValue;
			this.Wrap = false;
			this.Text = "0";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public double TickFrequency
		{
			get => TickDp.Get(this);
			set => TickDp.Set(this, value);
		}

		public bool Slider
		{
			get => SliderDp.Get(this);
			set => SliderDp.Set(this, value);
		}

		public double Minimum
		{
			get => MinDp.Get(this);
			set => MinDp.Set(this, value);
		}

		public double Maximum
		{
			get => MaxDp.Get(this);
			set => MaxDp.Set(this, value);
		}

		public bool Wrap
		{
			get => WrapDp.Get(this);
			set => WrapDp.Set(this, value);
		}

		public string Text
		{
			get
			{
				return this.inputString;
			}

			set
			{
				this.inputString = value;

				double val = 0;
				if (double.TryParse(value, out val))
				{
					this.Value = val;
					this.ErrorDisplay.Visibility = Visibility.Collapsed;
				}
				else
				{
					this.ErrorDisplay.Visibility = Visibility.Visible;
				}
			}
		}

		public double Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			bool focused = this.InputBox.IsKeyboardFocused || this.InputSlider.IsKeyboardFocused;
			if (!focused)
				return;

			if (e.Key == Key.Return)
			{
				this.Commit(true);
				e.Handled = true;
			}

			if (e.Key == Key.Up || e.Key == Key.Down)
			{
				e.Handled = true;

				if (e.IsRepeat)
				{
					if (this.keyHeld == e.Key)
						return;

					this.keyHeld = e.Key;
					Task.Run(this.TickHeldKey);
				}
				else
				{
					this.TickKey(e.Key);
				}
			}
		}

		protected override void OnPreviewKeyUp(KeyEventArgs e)
		{
			if (this.keyHeld == e.Key)
			{
				e.Handled = true;
				this.keyHeld = Key.None;
			}
		}

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			e.Handled = true;
			this.TickValue(e.Delta > 0);
		}

		private static void OnValueChanged(NumberBox sender, double v)
		{
			sender.Value = sender.Validate(v);
			sender.Text = sender.Value.ToString("0.##");
		}

		private double Validate(double v)
		{
			if (this.Wrap)
			{
				if (v > this.Maximum)
				{
					v = this.Minimum;
				}

				if (v < this.Minimum)
				{
					v = this.Maximum;
				}
			}
			else
			{
				v = Math.Min(v, this.Maximum);
				v = Math.Max(v, this.Minimum);
			}

			if (this.TickFrequency != 0)
				v = Math.Round(v / this.TickFrequency) * this.TickFrequency;

			return v;
		}

		private void OnLostFocus(object sender, RoutedEventArgs e)
		{
			this.Commit(false);
		}

		private void Commit(bool refocus)
		{
			try
			{
				this.Value = Convert.ToDouble(new DataTable().Compute(this.inputString, null));
				this.ErrorDisplay.Visibility = Visibility.Collapsed;
			}
			catch (Exception)
			{
				this.ErrorDisplay.Visibility = Visibility.Visible;
			}

			this.Text = this.Value.ToString("0.##");

			if (refocus)
			{
				this.InputBox.Focus();
				this.InputBox.CaretIndex = int.MaxValue;
			}
		}

		private async Task TickHeldKey()
		{
			while (this.keyHeld != Key.None)
			{
				await Application.Current.Dispatcher.InvokeAsync(() =>
				{
					this.TickKey(this.keyHeld);
				});

				await Task.Delay(10);
			}
		}

		private void TickKey(Key key)
		{
			if (key == Key.Up)
			{
				this.TickValue(true);
				this.Commit(true);
			}
			else if (key == Key.Down)
			{
				this.TickValue(false);
				this.Commit(true);
			}
		}

		private void TickValue(bool increase)
		{
			double delta = increase ? this.TickFrequency : -this.TickFrequency;

			if (Keyboard.IsKeyDown(Key.LeftShift))
				delta *= 10;

			double value = this.Value;
			double newValue = value + delta;
			newValue = this.Validate(newValue);

			if (newValue == value)
				return;

			this.Value = newValue;
		}
	}
}
