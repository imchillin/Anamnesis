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

	/// <summary>
	/// Interaction logic for NumberBox.xaml.
	/// </summary>
	public partial class NumberBox : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			nameof(Value),
			typeof(double),
			typeof(NumberBox),
			new FrameworkPropertyMetadata()
			{
				BindsTwoWayByDefault = true,
				DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged,
				PropertyChangedCallback = new PropertyChangedCallback(OnValueChangedStatic),
			});

		public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(NumberBox));
		public static readonly DependencyProperty SliderProperty = DependencyProperty.Register(
			nameof(Slider),
			typeof(bool),
			typeof(NumberBox),
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnChanged)));

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
			nameof(Minimum),
			typeof(double),
			typeof(NumberBox),
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnChanged)));

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			nameof(Maximum),
			typeof(double),
			typeof(NumberBox),
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnChanged)));

		public static readonly DependencyProperty WrapProperty = DependencyProperty.Register(
			nameof(Wrap),
			typeof(bool),
			typeof(NumberBox),
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnChanged)));

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
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public double TickFrequency
		{
			get
			{
				return (double)this.GetValue(TickFrequencyProperty);
			}
			set
			{
				this.SetValue(TickFrequencyProperty, value);
			}
		}

		public bool Slider
		{
			get
			{
				return (bool)this.GetValue(SliderProperty);
			}

			set
			{
				this.SetValue(SliderProperty, value);
			}
		}

		public double Minimum
		{
			get
			{
				return (double)this.GetValue(MinimumProperty);
			}

			set
			{
				this.SetValue(MinimumProperty, value);
			}
		}

		public double Maximum
		{
			get
			{
				return (double)this.GetValue(MaximumProperty);
			}

			set
			{
				this.SetValue(MaximumProperty, value);
			}
		}

		public bool Wrap
		{
			get
			{
				return (bool)this.GetValue(WrapProperty);
			}

			set
			{
				this.SetValue(WrapProperty, value);
			}
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
			get
			{
				return (double)this.GetValue(ValueProperty);
			}

			set
			{
				if (this.Wrap)
				{
					if (value > this.Maximum)
					{
						value = this.Minimum;
					}

					if (value < this.Minimum)
					{
						value = this.Maximum;
					}
				}
				else
				{
					value = Math.Min(value, this.Maximum);
					value = Math.Max(value, this.Minimum);
				}

				value = Math.Round(value / this.TickFrequency) * this.TickFrequency;

				this.SetValue(ValueProperty, value);
				this.Text = this.Value.ToString("0.###");
			}
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

		private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is NumberBox numberBox && e.Property.Name == nameof(Value))
			{
				numberBox.Text = numberBox.Value.ToString("0.##");
			}
		}

		private static void OnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is NumberBox numberBox)
			{
				numberBox.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
			}
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
			value += delta;

			this.Value = value;
		}
	}
}
