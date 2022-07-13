// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;
using WpfColor = System.Windows.Media.Color;

/// <summary>
/// Interaction logic for ColorControl.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class RgbaColorControl : UserControl
{
	public static readonly IBind<Color4> ValueDp = Binder.Register<Color4, RgbaColorControl>(nameof(Value), OnValueChanged);

	public RgbaColorControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public Color4 Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	private static void OnValueChanged(RgbaColorControl sender, Color4 value)
	{
		sender.UpdatePreview();
	}

	private static double Clamp(double v)
	{
		v = Math.Min(v, 1);
		v = Math.Max(v, 0);
		return v;
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
		ColorSelectorDrawer selector = new ColorSelectorDrawer();
		selector.EnableAlpha = true;
		selector.Value = this.Value;

		selector.ValueChanged += (v) =>
		{
			this.Value = v;
		};

		throw new NotImplementedException();
	}

	private void UpdatePreview()
	{
		WpfColor c = default;
		c.R = (byte)(Clamp(this.Value.R) * 255);
		c.G = (byte)(Clamp(this.Value.G) * 255);
		c.B = (byte)(Clamp(this.Value.B) * 255);
		c.A = (byte)(Clamp(this.Value.A) * 255);

		this.PreviewColor.Color = c;
	}
}
