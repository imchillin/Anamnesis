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
		public static readonly DependencyProperty SelectorProperty = DependencyProperty.Register(nameof(EnableSelector), typeof(bool), typeof(ColorEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

		public ColorEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[AlsoNotifyFor(nameof(ColorEditor.R), nameof(ColorEditor.G), nameof(ColorEditor.B))]
		public Color Value
		{
			get
			{
				return (Color)this.GetValue(ValueProperty);
			}

			set
			{
				this.SetValue(ValueProperty, value);
				this.UpdatePreview();
			}
		}

		[AlsoNotifyFor(nameof(ColorEditor.Value))]
		[DependsOn(nameof(ColorEditor.Value))]
		public float R
		{
			get
			{
				return this.Value.R;
			}

			set
			{
				this.Value = new Color(value, this.G, this.B);
				this.UpdatePreview();
			}
		}

		[AlsoNotifyFor(nameof(ColorEditor.Value))]
		[DependsOn(nameof(ColorEditor.Value))]
		public float G
		{
			get
			{
				return this.Value.G;
			}

			set
			{
				this.Value = new Color(this.R, value, this.B);
				this.UpdatePreview();
			}
		}

		[AlsoNotifyFor(nameof(ColorEditor.Value))]
		[DependsOn(nameof(ColorEditor.Value))]
		public float B
		{
			get
			{
				return this.Value.B;
			}

			set
			{
				this.Value = new Color(this.R, this.G, value);
				this.UpdatePreview();
			}
		}

		public bool EnableSelector
		{
			get
			{
				return (bool)this.GetValue(SelectorProperty);
			}

			set
			{
				this.SetValue(SelectorProperty, value);
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
					view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ColorEditor.R)));
					view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ColorEditor.G)));
					view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ColorEditor.B)));
				}

				view.UpdatePreview();
			}
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
