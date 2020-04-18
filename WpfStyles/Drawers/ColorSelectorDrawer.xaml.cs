// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Drawers
{
	using System;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;
	using ConceptMatrix.WpfStyles.DependencyProperties;

	using Binder = ConceptMatrix.WpfStyles.DependencyProperties.Binder;
	using CmColor = ConceptMatrix.Color;
	using WpfColor = System.Windows.Media.Color;

	/// <summary>
	/// Interaction logic for ColorSelectorDrawer.xaml.
	/// </summary>
	public partial class ColorSelectorDrawer : UserControl, IDrawer
	{
		public static readonly IBind<CmColor> ValueDp = Binder.Register<CmColor, ColorSelectorDrawer>(nameof(Value), OnValueChanged);
		public static readonly IBind<WpfColor> WpfColorDp = Binder.Register<WpfColor, ColorSelectorDrawer>(nameof(WpfColor), OnWpfColorChanged);

		public static readonly IBind<float> ChannelRDp = Binder.Register<float, ColorSelectorDrawer>(nameof(R), OnChanelChanged);
		public static readonly IBind<float> ChannelGDp = Binder.Register<float, ColorSelectorDrawer>(nameof(G), OnChanelChanged);
		public static readonly IBind<float> ChannelBDp = Binder.Register<float, ColorSelectorDrawer>(nameof(B), OnChanelChanged);

		private bool propertyLock = false;

		public ColorSelectorDrawer()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.List.Items.Add(new ColorOption(Colors.White, "White"));
			this.List.Items.Add(new ColorOption(Colors.Black, "Black"));

			PropertyInfo[] properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (PropertyInfo property in properties)
			{
				Color c = (Color)property.GetValue(null);

				if (c.A <= 0)
					continue;

				this.List.Items.Add(new ColorOption(c, property.Name));
			}
		}

		public delegate void ValueChangedEventHandler(CmColor value);

		public event ValueChangedEventHandler ValueChanged;
		public event DrawerEvent Close;

		public CmColor Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		public WpfColor WpfColor
		{
			get => WpfColorDp.Get(this);
			set => WpfColorDp.Set(this, value);
		}

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

		private static void OnValueChanged(ColorSelectorDrawer sender, CmColor value)
		{
			sender.ValueChanged?.Invoke(value);

			if (sender.propertyLock)
				return;

			sender.propertyLock = true;
			sender.WpfColor = ToWpfColor(value);
			sender.R = value.R;
			sender.G = value.G;
			sender.B = value.B;
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
			sender.propertyLock = false;
		}

		private static void OnChanelChanged(ColorSelectorDrawer sender, float value)
		{
			sender.Value = new CmColor(sender.R, sender.G, sender.B);
		}

		private static CmColor ToCmColor(WpfColor wpfColor)
		{
			return new CmColor(wpfColor.R / 255.0f, wpfColor.G / 255.0f, wpfColor.B / 255.0f);
		}

		private static float Clamp(float v)
		{
			v = Math.Min(v, 1);
			v = Math.Max(v, 0);
			return v;
		}

		private static WpfColor ToWpfColor(CmColor cmColor)
		{
			WpfColor v = default;
			v.A = 255;
			v.R = (byte)(Clamp(cmColor.R) * 255);
			v.G = (byte)(Clamp(cmColor.G) * 255);
			v.B = (byte)(Clamp(cmColor.B) * 255);
			return v;
		}

		private void ColorPicker_MouseMove(object sender, MouseEventArgs e)
		{
			this.propertyLock = true;
			this.Value = ToCmColor(this.Picker.Color);
			this.R = this.Value.R;
			this.G = this.Value.G;
			this.B = this.Value.B;
			this.propertyLock = false;
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ColorOption op = this.List.SelectedItem as ColorOption;
			this.Value = op.AsColor();
		}

		private class ColorOption
		{
			public ColorOption(Color c, string name)
			{
				this.Name = name;
				this.Color = c;
			}

			public Color Color { get; set; }
			public string Name { get; set; }

			public CmColor AsColor()
			{
				return new CmColor(this.Color.R / 255.0f, this.Color.G / 255.0f, this.Color.B / 255.0f);
			}
		}
	}
}
