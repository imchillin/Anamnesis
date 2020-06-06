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
	using Anamnesis;
	using ConceptMatrix.WpfStyles.DependencyProperties;

	using Binder = ConceptMatrix.WpfStyles.DependencyProperties.Binder;
	using WpfColor = System.Windows.Media.Color;

	/// <summary>
	/// Interaction logic for ColorSelectorDrawer.xaml.
	/// </summary>
	public partial class ColorSelectorDrawer : UserControl, IDrawer
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

			this.List.Items.Add(new ColorOption(Colors.White, "White"));
			this.List.Items.Add(new ColorOption(Colors.Black, "Black"));

			PropertyInfo[] properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (PropertyInfo property in properties)
			{
				WpfColor c = (WpfColor)property.GetValue(null);
				this.List.Items.Add(new ColorOption(c, property.Name));
			}
		}

		public delegate void ValueChangedEventHandler(Color4 value);

		public event ValueChangedEventHandler ValueChanged;
		public event DrawerEvent Close;

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

		public bool EnableAlpha
		{
			get => EnableAlphaDp.Get(this);
			set => EnableAlphaDp.Set(this, value);
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
			ColorOption op = this.List.SelectedItem as ColorOption;
			this.Value = op.AsColor();
		}

		private class ColorOption
		{
			public ColorOption(WpfColor c, string name)
			{
				this.Name = name;
				this.Color = c;
			}

			public WpfColor Color { get; set; }
			public string Name { get; set; }

			public Color4 AsColor()
			{
				return new Color4(this.Color.R / 255.0f, this.Color.G / 255.0f, this.Color.B / 255.0f, this.Color.A);
			}
		}
	}
}
