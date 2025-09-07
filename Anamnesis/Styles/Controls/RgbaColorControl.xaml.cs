// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;
using WpfColor = System.Windows.Media.Color;

/// <summary>
/// Interaction logic for RgbaColorControl.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class RgbaColorControl : UserControl
{
	/// <summary>Dependency property for the color value.</summary>
	public static readonly IBind<Color4> ValueDp = Binder.Register<Color4, RgbaColorControl>(nameof(Value), OnValueChanged);

	/// <summary>
	/// Initializes a new instance of the <see cref="RgbaColorControl"/> class.
	/// </summary>
	public RgbaColorControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	/// <summary>Gets or sets the RGBA color value.</summary>
	public Color4 Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	/// <summary>
	/// Called when the value of the color changes by an external source.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
	private static void OnValueChanged(RgbaColorControl sender, Color4 value)
	{
		sender.UpdatePreview();
	}

	/// <summary>
	/// Handles control click events, prompting the color selector drawer.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event args.</param>
	private async void OnClick(object sender, RoutedEventArgs e)
	{
		ColorSelectorDrawer selector = new()
		{
			EnableAlpha = true,
			Value = this.Value,
		};

		selector.ValueChanged += (v) => { this.Value = v; };

		await ViewService.ShowDrawer(selector);
	}

	/// <summary>Updates the color preview.</summary>
	private void UpdatePreview()
	{
		WpfColor c = default;
		c.R = (byte)(Color.Clamp(this.Value.R) * 255);
		c.G = (byte)(Color.Clamp(this.Value.G) * 255);
		c.B = (byte)(Color.Clamp(this.Value.B) * 255);
		c.A = (byte)(Color.Clamp(this.Value.A) * 255);

		this.PreviewColor.Color = c;
	}
}
