// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ColorEditor.xaml.
	/// </summary>
	public partial class ColorEditor : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Color), typeof(ColorEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));
		public static readonly DependencyProperty RProperty = DependencyProperty.Register(nameof(ValueR), typeof(double), typeof(ColorEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));
		public static readonly DependencyProperty GProperty = DependencyProperty.Register(nameof(ValueG), typeof(double), typeof(ColorEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));
		public static readonly DependencyProperty BProperty = DependencyProperty.Register(nameof(ValueB), typeof(double), typeof(ColorEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

		public ColorEditor()
		{
			this.InitializeComponent();
			this.Value = Colors.White;
			this.DataContext = this;

			this.RBox.PropertyChanged += this.OnRChanged;
			this.GBox.PropertyChanged += this.OnGChanged;
			this.BBox.PropertyChanged += this.OnBchanged;
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
				this.SetValue(ValueProperty, value);
				this.RBox.Value = this.ValueR;
				this.GBox.Value = this.ValueG;
				this.BBox.Value = this.ValueB;
			}
		}

		public double ValueR
		{
			get
			{
				return (double)this.GetValue(RProperty);
			}

			set
			{
				this.SetValue(RProperty, value);
				Color c = this.Value;
				c.R = (byte)(value * 255);
				this.Value = c;
			}
		}

		public double ValueG
		{
			get
			{
				return (double)this.GetValue(GProperty);
			}

			set
			{
				this.SetValue(GProperty, value);
				Color c = this.Value;
				c.G = (byte)(value * 255);
				this.Value = c;
			}
		}

		public double ValueB
		{
			get
			{
				return (double)this.GetValue(BProperty);
			}

			set
			{
				this.SetValue(BProperty, value);
				Color c = this.Value;
				c.B = (byte)(value * 255);
				this.Value = c;
			}
		}

		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is ColorEditor view)
			{
				view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
			}
		}

		private void OnRChanged(object sender, PropertyChangedEventArgs e)
		{
			this.ValueR = this.RBox.Value;
		}

		private void OnGChanged(object sender, PropertyChangedEventArgs e)
		{
			this.ValueG = this.GBox.Value;
		}

		private void OnBchanged(object sender, PropertyChangedEventArgs e)
		{
			this.ValueB = this.BBox.Value;
		}
	}
}
