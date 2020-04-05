// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using PropertyChanged;

	using Color = ConceptMatrix.Color;
	using WpfColor = System.Windows.Media.Color;
	using WpfColors = System.Windows.Media.Colors;

	/// <summary>
	/// Interaction logic for ColorEditor.xaml.
	/// </summary>
	public partial class ColorEditor : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Color), typeof(ColorEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

		private Color oldValue;

		public ColorEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Color Value
		{
			get
			{
				return (Color)this.GetValue(ValueProperty);
			}

			set
			{
				this.ListenToColor(value);
				this.SetValue(ValueProperty, value);
				this.UpdatePreview();
			}
		}

		private static double Clamp(double v)
		{
			v = Math.Min(v, 1);
			v = Math.Max(v, 0);
			return v;
		}

		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is ColorEditor view)
			{
				view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));

				if (e.Property.Name == nameof(Value))
				{
					view.ListenToColor(view.Value);
				}
			}
		}

		private void ListenToColor(Color value)
		{
			if (this.oldValue != null)
				this.oldValue.PropertyChanged -= this.Value_PropertyChanged;

			this.oldValue = value;
			value.PropertyChanged += this.Value_PropertyChanged;
			this.UpdatePreview();
		}

		private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ColorEditor.Value)));
			this.UpdatePreview();
		}

		private void UpdatePreview()
		{
			WpfColor c = default(WpfColor);
			c.R = (byte)(Clamp(this.Value.R) * 255);
			c.G = (byte)(Clamp(this.Value.G) * 255);
			c.B = (byte)(Clamp(this.Value.B) * 255);
			c.A = 255;

			this.Preview.Background = new SolidColorBrush(c);
		}
	}
}
