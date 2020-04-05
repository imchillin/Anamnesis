// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media.Media3D;

	/// <summary>
	/// Interaction logic for Vector3DEditor.xaml.
	/// </summary>
	public partial class Vector3DEditor : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Vector3D), typeof(Vector3DEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));
		public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(Vector3DEditor));

		public Vector3DEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public double Minimum { get; set; } = 0;
		public double Maximum { get; set; } = 100;

		public Vector3D Value
		{
			get
			{
				return (Vector3D)this.GetValue(ValueProperty);
			}

			set
			{
				this.SetValue(ValueProperty, value);
			}
		}

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

		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is Vector3DEditor view)
			{
				view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
			}
		}
	}
}
