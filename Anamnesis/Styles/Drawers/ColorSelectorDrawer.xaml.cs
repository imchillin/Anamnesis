// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Drawers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;
using Binder = XivToolsWpf.DependencyProperties.Binder;

using WpfColor = System.Windows.Media.Color;

/// <summary>
/// Interaction logic for ColorSelectorDrawer.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class ColorSelectorDrawer : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<Color4> ValueDp = Binder.Register<Color4, ColorSelectorDrawer>(nameof(Value), OnValueChanged);
	public static readonly IBind<WpfColor> WpfColorDp = Binder.Register<WpfColor, ColorSelectorDrawer>(nameof(WpfColor), OnWpfColorChanged);

	public static readonly IBind<bool> EnableAlphaDp = Binder.Register<bool, ColorSelectorDrawer>(nameof(EnableAlpha));

	public static readonly IBind<float> ChannelRDp = Binder.Register<float, ColorSelectorDrawer>(nameof(R), OnChanelChanged);
	public static readonly IBind<float> ChannelGDp = Binder.Register<float, ColorSelectorDrawer>(nameof(G), OnChanelChanged);
	public static readonly IBind<float> ChannelBDp = Binder.Register<float, ColorSelectorDrawer>(nameof(B), OnChanelChanged);
	public static readonly IBind<float> ChannelADp = Binder.Register<float, ColorSelectorDrawer>(nameof(A), OnChanelChanged);

	private bool propertyLock = false;
	private bool draggingPicker = false;

	public ColorSelectorDrawer()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		List<ColorOption> colors = new List<ColorOption>();
		PropertyInfo[] properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
		foreach (PropertyInfo property in properties)
		{
			if (property == null)
				continue;

			object? obj = property.GetValue(null);

			if (obj == null)
				continue;

			if (property.Name == "Transparent")
				continue;

			colors.Add(new ColorOption((WpfColor)obj, property.Name));
		}

		ColorConverter colorConverter = new ColorConverter();
		colors.Sort((a, b) =>
		{
			string? aHex = colorConverter.ConvertToString(a.Color);
			string? bHex = colorConverter.ConvertToString(b.Color);

			if (aHex == null || bHex == null)
				throw new Exception("Failed to convert colors to hex");

			return aHex.CompareTo(bHex);
		});

		foreach (ColorOption c in colors)
		{
			this.List.Items.Add(c);
		}

		if (FavoritesService.Colors != null)
		{
			foreach (Color4 color in FavoritesService.Colors)
			{
				ColorOption op = new ColorOption(color, string.Empty);
				this.RecentList.Items.Add(op);
			}
		}
	}

	public delegate void ValueChangedEventHandler(Color4 value);

	public event ValueChangedEventHandler? ValueChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	public Color4 Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public WpfColor WpfColor
	{
		get => WpfColorDp.Get(this);
		set => WpfColorDp.Set(this, value);
	}

	[AlsoNotifyFor(nameof(RByte))]
	public float R
	{
		get => ChannelRDp.Get(this);
		set => ChannelRDp.Set(this, value);
	}

	public float G
	{
		get => ChannelGDp.Get(this);
		set => ChannelGDp.Set(this, value);
	}

	public float B
	{
		get => ChannelBDp.Get(this);
		set => ChannelBDp.Set(this, value);
	}

	public float A
	{
		get => ChannelADp.Get(this);
		set => ChannelADp.Set(this, value);
	}

	public int RByte
	{
		get => (int)(ChannelRDp.Get(this) * 255);
		set => ChannelRDp.Set(this, value / 255.0f);
	}

	public int GByte
	{
		get => (int)(ChannelGDp.Get(this) * 255);
		set => ChannelGDp.Set(this, value / 255.0f);
	}

	public int BByte
	{
		get => (int)(ChannelBDp.Get(this) * 255);
		set => ChannelBDp.Set(this, value / 255.0f);
	}

	public int AByte
	{
		get => (int)(ChannelADp.Get(this) * 255);
		set => ChannelADp.Set(this, value / 255.0f);
	}

	public bool EnableAlpha
	{
		get => EnableAlphaDp.Get(this);
		set => EnableAlphaDp.Set(this, value);
	}

	public void Close()
	{
	}

	public void OnClosed()
	{
		if (FavoritesService.Colors != null)
		{
			foreach (Color4 color in FavoritesService.Colors)
			{
				if (color.IsApproximately(this.Value))
				{
					return;
				}
			}

			FavoritesService.Colors.Insert(0, this.Value);

			while (FavoritesService.Colors.Count > 12)
			{
				FavoritesService.Colors.RemoveAt(12);
			}

			FavoritesService.Save();
		}
	}

	private static void OnValueChanged(ColorSelectorDrawer sender, Color4 value)
	{
		sender.ValueChanged?.Invoke(value);

		if (sender.propertyLock)
			return;

		sender.propertyLock = true;
		sender.WpfColor = ToWpfColor(value);
		sender.R = value.R;
		sender.G = value.G;
		sender.B = value.B;
		sender.A = value.A;

		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(RByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(BByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(GByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AByte)));

		sender.propertyLock = false;
	}

	private static void OnWpfColorChanged(ColorSelectorDrawer sender, WpfColor value)
	{
		if (sender.propertyLock)
			return;

		sender.propertyLock = true;
		sender.Value = ToCmColor(value);
		sender.R = sender.Value.R;
		sender.G = sender.Value.G;
		sender.B = sender.Value.B;
		sender.A = sender.Value.A;

		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(RByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(BByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(GByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AByte)));

		sender.propertyLock = false;
	}

	private static void OnChanelChanged(ColorSelectorDrawer sender, float value)
	{
		sender.Value = new Color4(sender.R, sender.G, sender.B, sender.A);
	}

	private static Color4 ToCmColor(WpfColor wpfColor)
	{
		return new Color4(wpfColor.R / 255.0f, wpfColor.G / 255.0f, wpfColor.B / 255.0f, wpfColor.A / 255.0f);
	}

	private static float Clamp(float v)
	{
		v = Math.Min(v, 1);
		v = Math.Max(v, 0);
		return v;
	}

	private static WpfColor ToWpfColor(Color4 cmColor)
	{
		WpfColor v = default;
		v.R = (byte)(Clamp(cmColor.R) * 255);
		v.G = (byte)(Clamp(cmColor.G) * 255);
		v.B = (byte)(Clamp(cmColor.B) * 255);
		v.A = (byte)(Clamp(cmColor.A) * 255);
		return v;
	}

	private void Picker_MouseDown(object sender, MouseButtonEventArgs e)
	{
		this.draggingPicker = true;
	}

	private void Picker_MouseUp(object sender, MouseButtonEventArgs e)
	{
		this.draggingPicker = false;
	}

	private void ColorPicker_MouseMove(object sender, MouseEventArgs e)
	{
		if (!this.draggingPicker)
			return;

		this.propertyLock = true;
		Color4 val = ToCmColor(this.Picker.Color);
		val.A = this.Value.A;
		this.Value = val;
		this.R = this.Value.R;
		this.G = this.Value.G;
		this.B = this.Value.B;
		this.A = this.Value.A;
		this.propertyLock = false;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is ListBox list)
		{
			ColorOption? op = list.SelectedItem as ColorOption;

			if (op == null)
				return;

			this.Value = op.AsColor();

			if (list == this.List)
			{
				this.RecentList.SelectedItem = null;
			}
			else if (list == this.RecentList)
			{
				this.List.SelectedItem = null;
			}
		}
	}

	private class ColorOption
	{
		public ColorOption(WpfColor c, string name)
		{
			this.Name = name;
			this.Color = c;
		}

		public ColorOption(Color4 c, string name)
		{
			this.Name = name;

			WpfColor color = default(WpfColor);
			color.ScR = c.R;
			color.ScG = c.G;
			color.ScB = c.B;
			color.ScA = c.A;
			this.Color = color;
		}

		public WpfColor Color { get; set; }
		public string Name { get; set; }

		public Color4 AsColor()
		{
			return new Color4(this.Color.ScR, this.Color.ScG, this.Color.ScB, this.Color.ScA);
		}
	}
}
