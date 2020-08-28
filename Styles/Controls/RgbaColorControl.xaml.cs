// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using ConceptMatrix;
	using ConceptMatrix.Memory;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using ConceptMatrix.WpfStyles.Drawers;
	using PropertyChanged;
	using WpfColor = System.Windows.Media.Color;

	/// <summary>
	/// Interaction logic for ColorControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class RgbaColorControl : UserControl
	{
		public static readonly IBind<Color4> ValueDp = Binder.Register<Color4, RgbaColorControl>(nameof(Value), OnValueChanged);
		public static readonly IBind<string> NameDp = Binder.Register<string, RgbaColorControl>(nameof(DisplayName));

		public RgbaColorControl()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
			this.DisplayName = "Color";
		}

		public string DisplayName
		{
			get => NameDp.Get(this);
			set => NameDp.Set(this, value);
		}

		public Color4 Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		[SuppressPropertyChangedWarnings]
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

		private async void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();

			ColorSelectorDrawer selector = new ColorSelectorDrawer();
			selector.EnableAlpha = true;
			selector.Value = this.Value;

			selector.ValueChanged += (v) =>
			{
				this.Value = v;
			};

			await viewService.ShowDrawer(selector, this.DisplayName);
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
}
