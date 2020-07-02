// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using PropertyChanged;

	using Vector = Anamnesis.Vector;

	/// <summary>
	/// Interaction logic for Vector3DEditor.xaml.
	/// </summary>
	public partial class VectorEditor : UserControl, INotifyPropertyChanged
	{
		public static readonly IBind<bool> ExpandedDp = Binder.Register<bool, VectorEditor>(nameof(Expanded));
		public static readonly IBind<Vector> ValueDp = Binder.Register<Vector, VectorEditor>(nameof(Value), OnValueChanged);
		public static readonly IBind<double> TickFrequencyDp = Binder.Register<double, VectorEditor>(nameof(TickFrequency));
		public static readonly IBind<bool> WrapDp = Binder.Register<bool, VectorEditor>(nameof(Wrap));
		public static readonly IBind<bool> SlidersDp = Binder.Register<bool, VectorEditor>(nameof(Sliders));

		public VectorEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
			this.TickFrequency = 0.1;
			this.Sliders = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public double Minimum { get; set; } = 0;
		public double Maximum { get; set; } = 100;

		public bool Expanded
		{
			get => ExpandedDp.Get(this);
			set => ExpandedDp.Set(this, value);
		}

		public bool Sliders
		{
			get => SlidersDp.Get(this);
			set => SlidersDp.Set(this, value);
		}

		public Vector Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		public double TickFrequency
		{
			get => TickFrequencyDp.Get(this);
			set => TickFrequencyDp.Set(this, value);
		}

		public bool Wrap
		{
			get => WrapDp.Get(this);
			set => WrapDp.Set(this, value);
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

		[SuppressPropertyChangedWarnings]
		private static void OnValueChanged(VectorEditor sender, Vector value)
		{
			sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(VectorEditor.X)));
			sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(VectorEditor.Y)));
			sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(VectorEditor.Z)));
		}
	}
}
