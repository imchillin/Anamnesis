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
using CmColor = Anamnesis.Memory.Color;
using WpfColor = System.Windows.Media.Color;

/// <summary>
/// Interaction logic for ColorControl.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class RgbColorControl : UserControl
{
	public static readonly IBind<CmColor?> ValueDp = Binder.Register<CmColor?, RgbColorControl>(nameof(Value), OnValueChanged);

	public RgbColorControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.UpdatePreview();
	}

	public CmColor? Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	private static void OnValueChanged(RgbColorControl sender, CmColor? value)
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
		selector.EnableAlpha = false;

		if (this.Value == null)
			this.Value = new CmColor(1, 1, 1);

		selector.Value = new Color4((CmColor)this.Value);

		selector.ValueChanged += (v) =>
		{
			this.Value = v.Color;
		};

		throw new NotImplementedException();
	}

	private void UpdatePreview()
	{
		WpfColor c = default;

		if (this.Value != null)
		{
			CmColor color = (CmColor)this.Value;
			c.R = (byte)(Clamp(color.R) * 255);
			c.G = (byte)(Clamp(color.G) * 255);
			c.B = (byte)(Clamp(color.B) * 255);
			c.A = 255;
		}
		else
		{
			c.A = 0;
		}

		this.PreviewColor.Color = c;
	}
}
