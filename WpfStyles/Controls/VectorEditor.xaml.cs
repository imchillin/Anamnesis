// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using PropertyChanged;

	using Vector = ConceptMatrix.Vector;

	/// <summary>
	/// Interaction logic for Vector3DEditor.xaml.
	/// </summary>
	public partial class VectorEditor : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Vector), typeof(VectorEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));
		public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(VectorEditor));

		public VectorEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public double Minimum { get; set; } = 0;
		public double Maximum { get; set; } = 100;

		[AlsoNotifyFor(nameof(VectorEditor.X), nameof(VectorEditor.Y), nameof(VectorEditor.Z))]
		public Vector Value
		{
			get
			{
				return (Vector)this.GetValue(ValueProperty);
			}

			set
			{
				this.SetValue(ValueProperty, value);
			}
		}

		[AlsoNotifyFor(nameof(VectorEditor.Value))]
		[DependsOn(nameof(VectorEditor.Value))]
		public float X
		{
			get
			{
				return this.Value.X;
			}

			set
			{
				this.Value = new Vector(value, this.Y, this.Z);
			}
		}

		[AlsoNotifyFor(nameof(VectorEditor.Value))]
		[DependsOn(nameof(VectorEditor.Value))]
		public float Y
		{
			get
			{
				return this.Value.Y;
			}

			set
			{
				this.Value = new Vector(this.X, value, this.Z);
			}
		}

		[AlsoNotifyFor(nameof(VectorEditor.Value))]
		[DependsOn(nameof(VectorEditor.Value))]
		public float Z
		{
			get
			{
				return this.Value.Z;
			}

			set
			{
				this.Value = new Vector(this.X, this.Y, value);
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
			if (sender is VectorEditor view)
			{
				view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));

				if (e.Property.Name == nameof(Value))
				{
					view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(VectorEditor.X)));
					view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(VectorEditor.Y)));
					view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(VectorEditor.Z)));
				}
			}
		}
	}
}
