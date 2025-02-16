// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using PropertyChanged;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for Vector3DEditor.xaml.
/// </summary>
public partial class Vector2DEditor : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<bool> ExpandedDp = Binder.Register<bool, Vector2DEditor>(nameof(Expanded));
	public static readonly IBind<Vector2> ValueDp = Binder.Register<Vector2, Vector2DEditor>(nameof(Value), OnValueChanged);
	public static readonly IBind<double> TickFrequencyDp = Binder.Register<double, Vector2DEditor>(nameof(TickFrequency));
	public static readonly IBind<bool> WrapDp = Binder.Register<bool, Vector2DEditor>(nameof(Wrap));

	public Vector2DEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.TickFrequency = 0.1;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public double Minimum { get; set; } = 0;
	public double Maximum { get; set; } = 100;

	public bool Expanded
	{
		get => ExpandedDp.Get(this);
		set => ExpandedDp.Set(this, value);
	}

	public Vector2 Value
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

	[AlsoNotifyFor(nameof(Vector2DEditor.Value))]
	[DependsOn(nameof(Vector2DEditor.Value))]
	public float X
	{
		get => this.Value.X;
		set => this.Value = new Vector2(value, this.Y);
	}

	[AlsoNotifyFor(nameof(Vector2DEditor.Value))]
	[DependsOn(nameof(Vector2DEditor.Value))]
	public float Y
	{
		get => this.Value.Y;
		set => this.Value = new Vector2(this.X, value);
	}

	private static void OnValueChanged(Vector2DEditor sender, Vector2 value)
	{
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Vector2DEditor.X)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Vector2DEditor.Y)));
	}
}
