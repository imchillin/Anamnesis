// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using PropertyChanged;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for SliderInputBox.xaml
/// </summary>
/// <remarks>
/// An interactive input box that allows the user to slide horizontally to change the value.
/// Inspired by the components in Blender.
/// </remarks>
[AddINotifyPropertyChangedInterface]
public partial class SliderInputBox : UserControl
{
	/// <summary>The key for the dragging background color.</summary>
	const string DRAG_BG_KEY = "DraggingBackgroundColor";

	/// <summary>The key for the hover background color.</summary>
	const string HOVER_BG_KEY = "HoverBackgroundColor";

	/// <summary>The key for the normal background color.</summary>
	const string NORMAL_BG_KEY = "NormalBackgroundColor";

	/// <summary>The key for the disabled background color.</summary>
	const string DISABLED_BG_KEY = "DisabledBackgroundColor";

	/// <summary>Minimum distance for a drag action to be recognized.</summary>
	const decimal DRAG_MIN_DISTANCE = 0.01m;

	/// <summary>Multiplier for the key modifiers.</summary>
	const int KEY_MODIFIER_MULTIPLIER = 10;

	/// <summary>Delay for key repeat in milliseconds.</summary>
	const int KEY_REPEAT_DELAY = 10;

	/// <summary>Constant for floating-point number rounding to a whole number.</summary>
	const int INT_ROUNDING = 0;

	/// <summary>Dependency property for the slider value.</summary>
	public static readonly IBind<decimal> ValueDp = Binder.Register<decimal, SliderInputBox>(nameof(Value), OnValueChanged, BindMode.TwoWay);

	/// <summary>Dependency property for the default slider value.</summary>
	public static readonly IBind<decimal?> DefaultValueDp = Binder.Register<decimal?, SliderInputBox>(nameof(DefaultValue), BindMode.OneWay);

	/// <summary>Dependency property for the suffix text.</summary>
	public static readonly IBind<string> SuffixDp = Binder.Register<string, SliderInputBox>(nameof(Suffix), OnSuffixChanged, BindMode.OneWay);

	/// <summary>Dependency property for the minimum value.</summary>
	public static readonly IBind<decimal?> MinDp = Binder.Register<decimal?, SliderInputBox>(nameof(Minimum), OnMinimumChanged, BindMode.TwoWay);

	/// <summary>Dependency property for the maximum value.</summary>
	public static readonly IBind<decimal?> MaxDp = Binder.Register<decimal?, SliderInputBox>(nameof(Maximum), OnMaximumChanged, BindMode.TwoWay);

	/// <summary>Dependency property for the number of decimal places.</summary>
	public static readonly IBind<int?> DecimalPlacesDp = Binder.Register<int?, SliderInputBox>(nameof(DecimalPlaces), BindMode.OneWay);

	/// <summary>Dependency property for the overflow behavior.</summary>
	public static readonly IBind<OverflowModes> OverflowDp = Binder.Register<OverflowModes, SliderInputBox>(nameof(OverflowBehavior), BindMode.OneWay);

	/// <summary>Dependency property for the locked state.</summary>
	public static readonly IBind<bool> LockedDp = Binder.Register<bool, SliderInputBox>(nameof(Locked), BindMode.OneWay);

	/// <summary>Dependency property for the tick frequency.</summary>
	public static readonly IBind<decimal> TickFrequencyDp = Binder.Register<decimal, SliderInputBox>(nameof(TickFrequency), OnTickFrequencyChanged, BindMode.OneWay);

	/// <summary>Dependency property for the border color.</summary>
	public static readonly IBind<string?> BorderColorDp = Binder.Register<string?, SliderInputBox>(nameof(BorderColor), BindMode.OneWay);

	/// <summary>Dependency property for the visibility of the decrease and increase buttons.</summary>
	public static readonly IBind<bool> EnableStepButtonsDp = Binder.Register<bool, SliderInputBox>(nameof(EnableStepButtons), BindMode.OneWay);

	private Point startPoint;
	private bool isDragging = false;
	private Point originalMousePosition;
	private Key keyHeld = Key.None;
	private bool isInternalSet = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="SliderInputBox"/> class.
	/// </summary>
	public SliderInputBox()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		// Default property configuration
		this.TickFrequency = 0.01m;
		this.EnableStepButtons = true;
		this.Suffix = string.Empty;
		this.BorderColor = "#00000000";

		this.InputField.IsReadOnly = true;
		this.InputField.IsReadOnlyCaretVisible = false;

		this.OnPropertyChanged(nameof(this.Label));
	}

	/// <summary>Defines the overflow behavior modes.</summary>
	public enum OverflowModes
	{
		/// <summary>
		/// Clamps the value within the minimum and maximum values.
		/// </summary>
		Clamp,

		/// <summary>
		/// Loops the value around when it exceeds the minimum or maximum value.
		/// </summary>
		Loop,
	}

	/// <summary>Gets or sets the slider value.</summary>
	public decimal Value
	{
		get => ValueDp.Get(this);
		set
		{
			// Don't allow the value to be set if the control is locked
			if (this.Locked)
				return;

			decimal newVal = this.Validate(value);
			if (ValueDp.Get(this) == newVal)
				return;

			this.isInternalSet = true;
			ValueDp.Set(this, newVal);
			this.isInternalSet = false;
		}
	}

	/// <summary>Gets or sets the default slider value.</summary>
	public decimal? DefaultValue
	{
		get => DefaultValueDp.Get(this);
		set => DefaultValueDp.Set(this, value);
	}

	/// <summary>Gets the label text combining the value and suffix.</summary>

	[DependsOn(nameof(Value), nameof(Suffix))]
	public string Label => $"{this.FormatValue(this.Value)} {this.Suffix}";


	/// <summary>Gets or sets the suffix text.</summary>
	public string Suffix
	{
		get => SuffixDp.Get(this);
		set
		{
			this.isInternalSet = true;
			SuffixDp.Set(this, value);
			this.isInternalSet = false;
		}
	}


	/// <summary>Gets or sets a value indicating whether the input field is active.</summary>
	public bool IsInputFieldActive
	{
		get => !this.InputField.IsReadOnly;
		set
		{
			this.InputField.IsReadOnly = !value;
			this.ContentArea.Background = new SolidColorBrush((Color)this.FindResource(value || !this.IsMouseHovered ? NORMAL_BG_KEY : HOVER_BG_KEY));
		}
	}

	/// <summary>Gets or sets the tick frequency.</summary>
	public decimal TickFrequency
	{
		get => TickFrequencyDp.Get(this);
		set
		{
			this.isInternalSet = true;
			TickFrequencyDp.Set(this, value);
			this.isInternalSet = false;
		}
	}

	/// <summary>Gets or sets the minimum value.</summary>
	public decimal? Minimum
	{
		get => MinDp.Get(this);
		set
		{
			this.isInternalSet = true;
			MinDp.Set(this, value);
			this.isInternalSet = false;
		}
	}

	/// <summary>Gets or sets the maximum value.</summary>
	public decimal? Maximum
	{
		get => MaxDp.Get(this);
		set
		{
			this.isInternalSet = true;
			MaxDp.Set(this, value);
			this.isInternalSet = false;
		}
	}

	/// <summary>
	/// Gets or sets the number of decimal places to round to. Use "0" to round to the nearest whole number.
	/// </summary>
	public int? DecimalPlaces
	{
		get => DecimalPlacesDp.Get(this);
		set
		{
			Debug.Assert(value >= 0, "You cannot set a negative number to the decimal places property");
			DecimalPlacesDp.Set(this, value);
			this.Value = Math.Round(this.Value, value ?? INT_ROUNDING);
		}
	}

	/// <summary>Gets or sets the overflow behavior.</summary>
	public OverflowModes OverflowBehavior
	{
		get => OverflowDp.Get(this);
		set
		{
			if (value == OverflowModes.Clamp)
			{
				if (this.Minimum != null)
					this.DecreaseButton.IsEnabled = this.Value > this.Minimum;

				if (this.Maximum != null)
					this.IncreaseButton.IsEnabled = this.Value < this.Maximum;
			}
			else
			{
				this.DecreaseButton.IsEnabled = true;
				this.IncreaseButton.IsEnabled = true;
			}

			OverflowDp.Set(this, value);
		}
	}

	/// <summary>Gets or sets a value indicating whether the control is locked.</summary>
	public bool Locked
	{
		get => LockedDp.Get(this);
		set => LockedDp.Set(this, value);
	}

	/// <summary>Gets or sets the border color.</summary>
	public string? BorderColor
	{
		get => BorderColorDp.Get(this);
		set => BorderColorDp.Set(this, value);
	}

	/// <summary>Gets or sets a value indicating whether to enable the decrease and increase buttons.</summary>
	public bool EnableStepButtons
	{
		get => EnableStepButtonsDp.Get(this);
		set => EnableStepButtonsDp.Set(this, value);
	}

	/// <summary>
	/// Gets a value indicating whether the mouse is hovered over the user control.
	/// </summary>
	public bool IsMouseHovered { get; private set; } = false;

	/// <summary>
	/// Gets a value indicating whether to show the decrease and increase buttons.
	/// </summary>
	[DependsOn(nameof(IsMouseHovered), nameof(IsInputFieldActive))]
	public bool ShowStepButtons => this.EnableStepButtons && this.IsMouseHovered && !this.IsInputFieldActive;

	/// <summary>Handles changes to the slider value.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="val">The new value.</param>
	private static void OnValueChanged(SliderInputBox sender, decimal val)
	{
		if (sender.isInternalSet)
			return;

		sender.Value = sender.Validate(val);
		sender.OnPropertyChanged(nameof(Label));
	}

	/// <summary>Handles changes to the tick frequency.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
	private static void OnTickFrequencyChanged(SliderInputBox sender, decimal value)
	{
		if (sender.isInternalSet)
			return;

		Debug.Assert(value >= 0, "Tick frequency must be non-negative.");
		sender.TickFrequency = value;
	}

	/// <summary>Handles changes to the minimum value.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
	private static void OnMinimumChanged(SliderInputBox sender, decimal? value)
	{
		if (sender.isInternalSet)
			return;

		if (value != null && sender.Maximum != null && value > sender.Maximum)
		{
			sender.Minimum = sender.Maximum;
			return;
		}

		sender.Minimum = value;
	}

	/// <summary>Handles changes to the maximum value.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
	private static void OnMaximumChanged(SliderInputBox sender, decimal? value)
	{
		if (sender.isInternalSet)
			return;

		if (value != null && sender.Minimum != null && value < sender.Minimum)
		{
			sender.Maximum = sender.Minimum;
			return;
		}

		sender.Maximum = value;
	}

	/// <summary>
	/// Handles changes to the suffix text.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
	private static void OnSuffixChanged(SliderInputBox sender, string value)
	{
		if (sender.isInternalSet)
			return;

		sender.OnPropertyChanged(nameof(Label));
	}

	/// <summary>Handles the control's loaded event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnLoaded(object sender, RoutedEventArgs e) => this.AttachWindowEvents(true);

	/// <summary>Handles the control's unloaded event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnUnloaded(object sender, RoutedEventArgs e) => this.AttachWindowEvents(false);

	/// <summary>Handles the preview mouse button down event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.startPoint = e.GetPosition(this);
			this.originalMousePosition = this.PointToScreen(this.startPoint);
			this.isDragging = false;

			if (!this.IsInputFieldActive)
			{
				// Set the cursor clipping
				this.SetCursorClip(true);

				e.Handled = true;
				return;
			}
		}
		else if (e.MiddleButton == MouseButtonState.Pressed && this.DefaultValue != null)
		{
			if (!this.IsInputFieldActive)
			{
				this.Value = (decimal)this.DefaultValue;

				e.Handled = true;
				return;
			}
		}
	}

	/// <summary>Handles the preview mouse move event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnPreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && !this.IsInputFieldActive)
		{
			Point currentPoint = e.GetPosition(this);
			Vector diff = currentPoint - this.startPoint;

			if (diff.Length >= (double)DRAG_MIN_DISTANCE)
			{
				if (!this.isDragging)
				{
					Mouse.OverrideCursor = Cursors.None;
					this.ContentArea.Background = new SolidColorBrush((Color)this.FindResource(DRAG_BG_KEY));
				}


				this.isDragging = true;

				// Adjust the value based on horizontal movement
				decimal delta = (decimal)diff.X * this.TickFrequency;

				if (Keyboard.IsKeyDown(Key.LeftShift))
					delta *= KEY_MODIFIER_MULTIPLIER;

				if (Keyboard.IsKeyDown(Key.LeftCtrl))
					delta /= KEY_MODIFIER_MULTIPLIER;

				this.Value += delta;

				// Restore the cursor position
				SetCursorPos((int)this.originalMousePosition.X, (int)this.originalMousePosition.Y);
			}
		}
	}

	/// <summary>Handles the preview mouse left button up event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (!this.isDragging)
		{
			if (!this.IsInputFieldActive)
			{
				this.IsInputFieldActive = true;
				this.InputField.Focus();
				this.InputField.SelectAll();
			}

			Mouse.OverrideCursor = null;
		}
		else
		{
			Mouse.OverrideCursor = this.IsMouseHovered ? Cursors.SizeWE : null;
			this.ContentArea.Background = new SolidColorBrush((Color)this.FindResource(this.IsMouseHovered ? HOVER_BG_KEY : NORMAL_BG_KEY));
		}

		this.isDragging = false;

		// Remove the cursor clipping
		this.SetCursorClip(false);
	}

	/// <summary>Handles the mouse enter event for the content area.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		this.IsMouseHovered = true;
		if (this.IsMouseHovered && !this.IsInputFieldActive)
		{
			this.ContentArea.Background = new SolidColorBrush((Color)this.FindResource(HOVER_BG_KEY));
		}
	}

	/// <summary>Handles the mouse leave event for the content area.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		this.IsMouseHovered = false;
		if (!this.IsInputFieldActive)
		{
			this.ContentArea.Background = new SolidColorBrush((Color)this.FindResource(NORMAL_BG_KEY));
		}
	}

	/// <summary>Handles the lost focus event for the input field.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnLostFocus(object sender, RoutedEventArgs e)
	{
		if (this.IsInputFieldActive)
		{
			string inputText = this.InputField.Text.Replace(',', '.');
			if (decimal.TryParse(inputText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal newValue))
			{
				this.Value = newValue;
				this.InputField.Text = this.Value.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				this.InputField.Text = this.Value.ToString(CultureInfo.InvariantCulture);
			}
		}

		this.IsInputFieldActive = false;

		// Ensure that we clean up the cursor state changes if the mouse leaves the control
		Mouse.OverrideCursor = this.IsMouseHovered ? Cursors.SizeWE : null;
		this.SetCursorClip(false);
	}

	/// <summary>Handles the preview key down event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		base.OnPreviewKeyDown(e);

		if (this.IsInputFieldActive && e.Key == Key.Enter)
		{
			string inputText = this.InputField.Text.Replace(',', '.');
			if (decimal.TryParse(inputText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal newValue))
			{
				this.Value = newValue;
				this.InputField.Text = this.Value.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				this.InputField.Text = this.Value.ToString(CultureInfo.InvariantCulture);
			}

			// Commit the changes
			Keyboard.ClearFocus();
			this.IsInputFieldActive = false;
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

	/// <summary>Handles the decrease button click event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnDecreaseButtonClicked(object sender, RoutedEventArgs e)
	{
		if (this.isDragging)
			return;

		decimal delta = this.TickFrequency;

		if (Keyboard.IsKeyDown(Key.LeftShift))
			delta *= KEY_MODIFIER_MULTIPLIER;

		if (Keyboard.IsKeyDown(Key.LeftCtrl))
			delta /= KEY_MODIFIER_MULTIPLIER;

		this.Value -= delta;
	}

	/// <summary>Handles the increase button click event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnIncreaseButtonClicked(object sender, RoutedEventArgs e)
	{
		if (this.isDragging)
			return;

		decimal delta = this.TickFrequency;

		if (Keyboard.IsKeyDown(Key.LeftShift))
			delta *= KEY_MODIFIER_MULTIPLIER;

		if (Keyboard.IsKeyDown(Key.LeftCtrl))
			delta /= KEY_MODIFIER_MULTIPLIER;

		this.Value += delta;
	}

	/// <summary>Handles the input area mouse enter event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnInputAreaMouseEnter(object sender, MouseEventArgs e)
	{
		if (!this.IsInputFieldActive)
		{
			Mouse.OverrideCursor = Cursors.SizeWE;
		}
	}

	/// <summary>Handles the input area mouse leave event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnInputAreaMouseLeave(object sender, MouseEventArgs e)
	{
		Mouse.OverrideCursor = null;
		this.SetCursorClip(false);
	}

	/// <summary>Handles the preview key up event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void OnPreviewKeyUp(object sender, KeyEventArgs e)
	{
		base.OnPreviewKeyUp(e);

		if (this.keyHeld == e.Key)
		{
			e.Handled = true;
			this.keyHeld = Key.None;
		}
	}

	/// <summary>Handles the mouse wheel event.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void OnMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (this.IsInputFieldActive)
			return;

		this.TickValue(e.Delta > 0);
		e.Handled = true;
	}

	/// <summary>Handles the is enabled changed event to update the background color.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
			this.ContentArea.Background = new SolidColorBrush((Color)this.FindResource(this.IsMouseHovered ? HOVER_BG_KEY : NORMAL_BG_KEY));
		else
			this.ContentArea.Background = new SolidColorBrush((Color)this.FindResource(DISABLED_BG_KEY));
	}

	/// <summary>
	/// Handles the tick value change based on the direction.
	/// </summary>
	/// <param name="increase">If set to <c>true</c> increases the value; otherwise, decreases it.</param>
	private void TickValue(bool increase)
	{
		decimal delta = increase ? this.TickFrequency : -this.TickFrequency;

		if (Keyboard.IsKeyDown(Key.LeftShift))
			delta *= KEY_MODIFIER_MULTIPLIER;

		if (Keyboard.IsKeyDown(Key.LeftCtrl))
			delta /= KEY_MODIFIER_MULTIPLIER;

		this.Value += delta;
	}

	/// <summary>
	/// Continuously ticks the held key to change the value.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	private async Task TickHeldKey()
	{
		while (this.keyHeld != Key.None)
		{
			await Application.Current.Dispatcher.InvokeAsync(() =>
			{
				this.TickKey(this.keyHeld);
			});

			await Task.Delay(KEY_REPEAT_DELAY);
		}
	}

	/// <summary>
	/// Adjusts the value based on the pressed key.
	/// </summary>
	/// <param name="key">The key that was pressed.</param>
	private void TickKey(Key key)
	{
		if (key == Key.Up)
		{
			this.TickValue(true);
		}
		else if (key == Key.Down)
		{
			this.TickValue(false);
		}
	}

	/// <summary>
	/// Validates and adjusts the new value based on the overflow behavior and decimal places.
	/// </summary>
	/// <param name="newVal">The new value to validate.</param>
	/// <returns>The validated value.</returns>
	private decimal Validate(decimal newVal)
	{
		decimal val = this.Value;

		// Round the value to the specified number of decimal places
		val = Math.Round(newVal, this.DecimalPlaces ?? INT_ROUNDING);

		if (this.OverflowBehavior == OverflowModes.Clamp)
		{
			decimal min = this.Minimum ?? decimal.MinValue;
			decimal max = this.Maximum ?? decimal.MaxValue;
			val = Math.Clamp(val, min, max);

			this.DecreaseButton.IsEnabled = val > min;
			this.IncreaseButton.IsEnabled = val < max;
		}
		else if (this.OverflowBehavior == OverflowModes.Loop && this.Minimum != null && this.Maximum != null)
		{
			if (val < this.Minimum)
				val = (decimal)this.Maximum;
			else if (val > this.Maximum)
				val = (decimal)this.Minimum;
		}

		return val;
	}

	/// <summary>
	/// Formats the value to a string with the specified number of decimal places.
	/// </summary>
	/// <param name="value">The value to format.</param>
	/// <returns>The the formatted value as string.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string FormatValue(decimal value)
	{
		int decimalPlaces = this.DecimalPlaces ?? INT_ROUNDING;
		string format = $"0.{new string('#', decimalPlaces)}";
		return value.ToString(format, CultureInfo.InvariantCulture);
	}

	/// <summary>Attaches or detaches window events.</summary>
	private void AttachWindowEvents(bool attach)
	{
		Window window = Window.GetWindow(this);
		if (window != null)
		{
			if (attach)
			{
				window.MouseDown += this.OnWindowEvent;
				window.Deactivated += this.OnWindowEvent;
			}
			else
			{
				window.MouseDown -= this.OnWindowEvent;
				window.Deactivated -= this.OnWindowEvent;
			}
		}
	}

	/// <summary>Handles window events to clear focus.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnWindowEvent(object? sender, EventArgs e)
	{
		FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), null);
		Keyboard.ClearFocus();
	}

	/// <summary>
	/// Sets or removes the cursor clipping to restrict cursor movement within the input area.
	/// </summary>
	/// <param name="clip">If set to <c>true</c> sets the cursor clipping; otherwise, removes it.</param>
	private void SetCursorClip(bool clip)
	{
		if (clip)
		{
			// Get the position of the InputArea grid on the screen
			int margin = this.ShowStepButtons ? 2 : 1;
			Point topLeft = this.InputArea.PointToScreen(new Point(margin, margin));
			Point bottomRight = this.InputArea.PointToScreen(new Point(this.InputArea.ActualWidth - margin, this.InputArea.ActualHeight - margin));

			// Define the clipping rectangle
			RECT rect = new()
			{
				Left = (int)topLeft.X,
				Top = (int)topLeft.Y,
				Right = (int)bottomRight.X,
				Bottom = (int)bottomRight.Y
			};

			// Set the cursor clipping rectangle to prevent the cursor from leaving the InputArea bounding rectangle
			ClipCursor(ref rect);
		}
		else
		{
			// Remove the cursor clipping
			ClipCursor(IntPtr.Zero);
		}
	}

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetCursorPos(int X, int Y);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ClipCursor(ref RECT lpRect);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ClipCursor(IntPtr lpRect);

	[StructLayout(LayoutKind.Sequential)]
	private struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}
}
