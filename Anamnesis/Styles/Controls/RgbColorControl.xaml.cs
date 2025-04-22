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
using CmColor = Anamnesis.Memory.Color;
using WpfColor = System.Windows.Media.Color;

/// <summary>
/// Interaction logic for RgbColorControl.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class RgbColorControl : UserControl
{
	/// <summary>Dependency property for the color value.</summary>
	public static readonly IBind<CmColor?> ValueDp = Binder.Register<CmColor?, RgbColorControl>(nameof(Value), OnValueChanged);

	/// <summary>
	/// Initializes a new instance of the <see cref="RgbColorControl"/> class.
	/// </summary>
	public RgbColorControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.UpdatePreview();
	}

	/// <summary>Gets or sets the RGB color value.</summary>
	public CmColor? Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	/// <summary>
	/// Called when the value of the color changes by an external source.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="value">The new value.</param>
	private static void OnValueChanged(RgbColorControl sender, CmColor? value)
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
		ColorSelectorDrawer selector = new() { EnableAlpha = false };
		selector.ValueChanged += (v) => { this.Value = v.Color; };

		if (!this.Value.HasValue)
			this.Value = CmColor.White;

		selector.Value = new Color4(this.Value.Value);

		await ViewService.ShowDrawer(selector);
	}

	/// <summary>Updates the color preview.</summary>
	private void UpdatePreview()
	{
		WpfColor c = default;

		if (this.Value.HasValue)
		{
			CmColor color = this.Value.Value;
			c.R = (byte)(CmColor.Clamp(color.R) * 255);
			c.G = (byte)(CmColor.Clamp(color.G) * 255);
			c.B = (byte)(CmColor.Clamp(color.B) * 255);
			c.A = 255;
		}
		else
		{
			c.A = 0;
		}

		this.PreviewColor.Color = c;
	}
}
