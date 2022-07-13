// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Actor.Utilities;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;
using WpfColor = System.Windows.Media.Color;

/// <summary>
/// Interaction logic for ColorControl.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class ColorControl : UserControl
{
	public static readonly IBind<byte> ValueDp = Binder.Register<byte, ColorControl>(nameof(Value), OnValueChanged);
	public static readonly IBind<ActorCustomizeMemory.Genders> GenderDp = Binder.Register<ActorCustomizeMemory.Genders, ColorControl>(nameof(Gender), OnGenderChanged);
	public static readonly IBind<ActorCustomizeMemory.Tribes> TribeDp = Binder.Register<ActorCustomizeMemory.Tribes, ColorControl>(nameof(Tribe), OnTribeChanged);
	public static readonly IBind<string> ToolTipKeyDp = Binder.Register<string, ColorControl>(nameof(ToolTipKey));

	private ColorData.Entry[]? colors;

	public ColorControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public enum ColorType
	{
		Skin,
		Eyes,
		Lips,
		FacePaint,
		Hair,
		HairHighlights,
	}

	public ColorType Type
	{
		get;
		set;
	}

	public ActorCustomizeMemory.Genders Gender
	{
		get => GenderDp.Get(this);
		set => GenderDp.Set(this, value);
	}

	public ActorCustomizeMemory.Tribes Tribe
	{
		get => TribeDp.Get(this);
		set => TribeDp.Set(this, value);
	}

	[AlsoNotifyFor(nameof(WpfColor))]
	public byte Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public string ToolTipKey
	{
		get => ToolTipKeyDp.Get(this);
		set => ToolTipKeyDp.Set(this, value);
	}

	public WpfColor WpfColor
	{
		get
		{
			if (this.colors == null || this.colors.Length <= 0 || this.Value >= this.colors.Length)
				return System.Windows.Media.Colors.Transparent;

			return this.colors[this.Value].WpfColor;
		}
	}

	private static void OnGenderChanged(ColorControl sender, ActorCustomizeMemory.Genders value)
	{
		sender.colors = sender.GetColors();
		sender.PreviewColor.Color = sender.WpfColor;
	}

	private static void OnTribeChanged(ColorControl sender, ActorCustomizeMemory.Tribes value)
	{
		if (!Enum.IsDefined<ActorCustomizeMemory.Tribes>(value))
			return;

		sender.colors = sender.GetColors();
		sender.PreviewColor.Color = sender.WpfColor;
	}

	private static void OnValueChanged(ColorControl sender, byte value)
	{
		sender.PreviewColor.Color = sender.WpfColor;
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
		if (this.colors == null)
			return;

		FxivColorSelectorDrawer selector = new FxivColorSelectorDrawer(this.colors, this.Value);

		selector.SelectionChanged += (v) =>
		{
			if (selector.Selected < 0 || selector.Selected >= this.colors.Length)
				return;

			this.Value = (byte)v;
		};

		throw new NotImplementedException();
	}

	private ColorData.Entry[]? GetColors()
	{
		if (!Enum.IsDefined(this.Tribe))
			return null;

		switch (this.Type)
		{
			case ColorType.Skin: return ColorData.GetSkin(this.Tribe, this.Gender);
			case ColorType.Eyes: return ColorData.GetEyeColors();
			case ColorType.Lips: return ColorData.GetLipColors();
			case ColorType.FacePaint: return ColorData.GetFacePaintColor();
			case ColorType.Hair: return ColorData.GetHair(this.Tribe, this.Gender);
			case ColorType.HairHighlights: return ColorData.GetHairHighlights();
		}

		throw new Exception("Unsupported color type: " + this.Type);
	}
}
