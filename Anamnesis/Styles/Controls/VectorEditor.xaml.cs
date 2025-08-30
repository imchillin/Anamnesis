// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using PropertyChanged;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Math3D.Extensions;

/// <summary>
/// Interaction logic for VectorEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class VectorEditor : UserControl, INotifyPropertyChanged
{
	/// <summary>Gets or sets the layout style.</summary>
	public static readonly IBind<LayoutStyles> LayoutStyleDp = Binder.Register<LayoutStyles, VectorEditor>(nameof(LayoutStyle));

	/// <summary>Gets or sets the vector value.</summary>
	public static readonly IBind<Vector3> ValueDp = Binder.Register<Vector3, VectorEditor>(nameof(Value), OnValueChanged);

	/// <summary>Gets or sets the default value.</summary>
	public static readonly IBind<decimal?> DefaultValueDp = Binder.Register<decimal?, VectorEditor>(nameof(DefaultValue), BindMode.OneWay);

	/// <summary>Gets or sets the tick frequency.</summary>
	public static readonly IBind<decimal> TickFrequencyDp = Binder.Register<decimal, VectorEditor>(nameof(TickFrequency));

	/// <summary>Gets or sets the minimum value.</summary>
	public static readonly IBind<string?> MinDp = Binder.Register<string?, VectorEditor>(nameof(Minimum));

	/// <summary>Gets or sets the maximum value.</summary>
	public static readonly IBind<string?> MaxDp = Binder.Register<string?, VectorEditor>(nameof(Maximum));

	/// <summary>Gets or sets a value indicating whether linking can be enabled.</summary>
	public static readonly IBind<bool> CanLinkDp = Binder.Register<bool, VectorEditor>(nameof(CanLink), BindMode.OneWay);

	/// <summary>Gets or sets a value indicating whether the axes are linked.</summary>
	public static readonly IBind<bool> LinkedDp = Binder.Register<bool, VectorEditor>(nameof(Linked));

	/// <summary>Gets or sets the color mode.</summary>
	public static readonly IBind<ColorModes> ColorModeDp = Binder.Register<ColorModes, VectorEditor>(nameof(ColorMode), BindMode.OneWay);

	/// <summary>Gets or sets the suffix.</summary>
	public static readonly IBind<string> SuffixDp = Binder.Register<string, VectorEditor>(nameof(Suffix));

	/// <summary>Gets or sets a value indicating whether step buttons are enabled.</summary>
	public static readonly IBind<bool> EnableStepButtonsDp = Binder.Register<bool, VectorEditor>(nameof(EnableStepButtons));

	/// <summary>Gets or sets the number of decimal places.</summary>
	public static readonly IBind<int> DecimalPlacesDp = Binder.Register<int, VectorEditor>(nameof(DecimalPlaces));

	/// <summary>Gets or sets the overflow behavior.</summary>
	public static readonly IBind<SliderInputBox.OverflowModes> OverflowModeDp = Binder.Register<SliderInputBox.OverflowModes, VectorEditor>(nameof(OverflowBehavior));

	/// <summary>Dependency property for the slider mode.</summary>
	public static readonly IBind<SliderInputBox.SliderModes> SliderModeDp = Binder.Register<SliderInputBox.SliderModes, VectorEditor>(nameof(SliderMode), BindMode.OneWay);

	/// <summary>Dependency property for the classic slider mode.</summary>
	public static readonly IBind<SliderInputBox.SliderTypes> SliderTypeDp = Binder.Register<SliderInputBox.SliderTypes, VectorEditor>(nameof(SliderType), BindMode.OneWay);

	/// <summary>Dependency property for the visibility of the tick visualizer.</summary>
	/// <remarks>Applies only to the standard slider mode.</remarks>
	public static readonly IBind<bool> ShowSliderThumbDp = Binder.Register<bool, VectorEditor>(nameof(ShowSliderThumb), BindMode.OneWay);

	private bool lockChangedEvent = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="VectorEditor"/> class.
	/// </summary>
	public VectorEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.LayoutStyle = LayoutStyles.Standard;
		this.TickFrequency = 0.01m;
		this.ColorMode = ColorModes.Standard;
		this.Suffix = "";
		this.EnableStepButtons = true;
		this.DecimalPlaces = 0;
		this.OverflowBehavior = SliderInputBox.OverflowModes.Clamp;
		this.SliderMode = SliderInputBox.SliderModes.Absolute;
		this.SliderType = SliderInputBox.SliderTypes.Modern;
		this.ShowSliderThumb = false;
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Defines the color modes for the vector editor.</summary>
	public enum ColorModes
	{
		/// <summary>Transparent Borders.</summary>
		Standard,

		/// <summary>RGB Borders.</summary>
		Rotation,
	}

	/// <summary>Defines the layout styles for the vector editor.</summary>
	public enum LayoutStyles
	{
		/// <summary>
		/// Each axis' slider is placed on a new line.
		/// </summary>
		Standard,

		/// <summary>
		/// Each axis' slider is placed on the same line, but all secondary elements are placed on a new line.
		/// </summary>
		Inline,

		/// <summary>
		/// Each axis' slider and secondary elements are placed on the same line.
		/// </summary>
		Compact,
	}

	/// <summary>Gets or sets the minimum value.</summary>
	public string? Minimum
	{
		get => MinDp.Get(this);
		set => MinDp.Set(this, value);
	}

	/// <summary>Gets or sets the maximum value.</summary>
	public string? Maximum
	{
		get => MaxDp.Get(this);
		set => MaxDp.Set(this, value);
	}

	/// <summary>Gets or sets the layout style.</summary>
	public LayoutStyles LayoutStyle
	{
		get => LayoutStyleDp.Get(this);
		set => LayoutStyleDp.Set(this, value);
	}

	/// <summary>
	/// Gets a value indicating whether the layout style is Standard.
	/// </summary>
	[DependsOn(nameof(LayoutStyle))]
	public bool IsStandardLayout => this.LayoutStyle == LayoutStyles.Standard;

	/// <summary>
	/// Gets a value indicating whether the layout style is Inline.
	/// </summary>
	[DependsOn(nameof(LayoutStyle))]
	public bool IsInlineLayout => this.LayoutStyle == LayoutStyles.Inline;

	/// <summary>
	/// Gets a value indicating whether the layout style is Compact.
	/// </summary>
	[DependsOn(nameof(LayoutStyle))]
	public bool IsCompactLayout => this.LayoutStyle == LayoutStyles.Compact;

	/// <summary>Gets or sets the vector value.</summary>
	public Vector3 Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	/// <summary>Gets or sets the default value.</summary>
	public decimal? DefaultValue
	{
		get => DefaultValueDp.Get(this);
		set => DefaultValueDp.Set(this, value);
	}

	/// <summary>Gets or sets the tick frequency.</summary>
	public decimal TickFrequency
	{
		get => TickFrequencyDp.Get(this);
		set => TickFrequencyDp.Set(this, value);
	}

	/// <summary>Gets or sets a value indicating whether linking can be enabled.</summary>
	public bool CanLink
	{
		get => CanLinkDp.Get(this);
		set => CanLinkDp.Set(this, value);
	}

	/// <summary>Gets or sets a value indicating whether the axes are linked.</summary>
	public bool Linked
	{
		get => LinkedDp.Get(this);
		set => LinkedDp.Set(this, value);
	}

	/// <summary>Gets or sets the color mode.</summary>
	public ColorModes ColorMode
	{
		get => ColorModeDp.Get(this);
		set
		{
			ColorModeDp.Set(this, value);

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.XColor)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.YColor)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ZColor)));
		}
	}

	/// <summary>Gets or sets the suffix.</summary>
	public string Suffix
	{
		get => SuffixDp.Get(this);
		set => SuffixDp.Set(this, value);
	}

	/// <summary>Gets or sets a value indicating whether step buttons are enabled.</summary>
	public bool EnableStepButtons
	{
		get => EnableStepButtonsDp.Get(this);
		set => EnableStepButtonsDp.Set(this, value);
	}

	/// <summary>Gets or sets the number of decimal places.</summary>
	public int DecimalPlaces
	{
		get => DecimalPlacesDp.Get(this);
		set => DecimalPlacesDp.Set(this, value);
	}

	/// <summary>Gets or sets the overflow behavior.</summary>
	public SliderInputBox.OverflowModes OverflowBehavior
	{
		get => OverflowModeDp.Get(this);
		set => OverflowModeDp.Set(this, value);
	}

	/// <summary>Gets or sets the slider mode.</summary>
	public SliderInputBox.SliderModes SliderMode
	{
		get => SliderModeDp.Get(this);
		set => SliderModeDp.Set(this, value);
	}

	/// <summary>Gets or sets a value indicating whether the classic slider mode is enabled.</summary>
	public SliderInputBox.SliderTypes SliderType
	{
		get => SliderTypeDp.Get(this);
		set => SliderTypeDp.Set(this, value);
	}

	/// <summary>Gets or sets a value indicating whether the tick visualizer is shown.</summary>
	public bool ShowSliderThumb
	{
		get => ShowSliderThumbDp.Get(this);
		set => ShowSliderThumbDp.Set(this, value);
	}

	/// <summary>Gets the X-axis color based on the color mode.</summary>
	[DependsOn(nameof(ColorMode))]
	public string XColor => (this.ColorMode == ColorModes.Rotation) ? "#2861FD" : "";

	/// <summary>Gets the Y-axis color based on the color mode.</summary>
	[DependsOn(nameof(ColorMode))]
	public string YColor => (this.ColorMode == ColorModes.Rotation) ? "#8DDB04" : "";

	/// <summary>Gets the Z-axis color based on the color mode.</summary>
	[DependsOn(nameof(ColorMode))]
	public string ZColor => (this.ColorMode == ColorModes.Rotation) ? "#FF1746" : "";

	/// <summary>Gets or sets the X value of the vector.</summary>
	[AlsoNotifyFor(nameof(Value))]
	[DependsOn(nameof(Value))]
	public float X
	{
		get => this.Value.X;
		set
		{
			if (this.Linked)
			{
				float delta = value - this.Value.X;
				this.Value = new Vector3(value, this.Value.Y + delta, this.Value.Z + delta);
			}
			else
			{
				this.Value = new Vector3(value, this.Y, this.Z);
			}
		}
	}

	/// <summary>Gets or sets the Y value of the vector.</summary>
	[AlsoNotifyFor(nameof(Value))]
	[DependsOn(nameof(Value))]
	public float Y
	{
		get => this.Value.Y;
		set
		{
			if (this.Linked)
			{
				float delta = value - this.Value.Y;
				this.Value = new Vector3(this.Value.X + delta, value, this.Value.Z + delta);
			}
			else
			{
				this.Value = new Vector3(this.X, value, this.Z);
			}
		}
	}

	/// <summary>Gets or sets the Z value of the vector.</summary>
	[AlsoNotifyFor(nameof(Value))]
	[DependsOn(nameof(Value))]
	public float Z
	{
		get => this.Value.Z;
		set
		{
			if (this.Linked)
			{
				float delta = value - this.Value.Z;
				this.Value = new Vector3(this.Value.X + delta, this.Value.Y + delta, value);
			}
			else
			{
				this.Value = new Vector3(this.X, this.Y, value);
			}
		}
	}

	private static void OnValueChanged(VectorEditor sender, Vector3 oldValue, Vector3 newValue)
	{
		if (sender.Linked && !sender.lockChangedEvent)
		{
			sender.lockChangedEvent = true;
			Vector3 deltaV = newValue - oldValue;
			float delta = Math.Max(deltaV.X, Math.Max(deltaV.Y, deltaV.Z));

			if (delta == 0)
				delta = Math.Min(deltaV.X, Math.Min(deltaV.Y, deltaV.Z));

			sender.Value = VectorExtensions.Add(oldValue, delta);
			sender.lockChangedEvent = false;
		}

		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(X)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Y)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Z)));
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Tab)
		{
			var sliders = VisualUtils.FindVisualChildren<SliderInputBox>(this)
				.Where(s => s.IsVisible && s.IsEnabled)
				.ToList();

			if (sliders.Count == 0)
				return;

			int currentIndex = sliders.FindIndex(s => s.IsInputFieldActive);

			if (currentIndex >= 0)
				sliders[currentIndex].LoseFocus();

			bool backwards = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			int nextIndex = (currentIndex + (backwards ? sliders.Count - 1 : 1)) % sliders.Count;
			sliders[nextIndex].GainFocus(true);

			e.Handled = true;
		}
	}
}
