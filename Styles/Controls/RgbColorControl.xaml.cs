// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using Anamnesis;
	using ConceptMatrix;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using ConceptMatrix.WpfStyles.Drawers;
	using PropertyChanged;

	using CmColor = Anamnesis.Color;
	using WpfColor = System.Windows.Media.Color;

	/// <summary>
	/// Interaction logic for ColorControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class RgbColorControl : UserControl
	{
		public static readonly IBind<CmColor?> ValueDp = Binder.Register<CmColor?, RgbColorControl>(nameof(Value), OnValueChanged);
		public static readonly IBind<string> NameDp = Binder.Register<string, RgbColorControl>(nameof(DisplayName));

		public RgbColorControl()
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

		public CmColor? Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		[SuppressPropertyChangedWarnings]
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

		private async void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();

			ColorSelectorDrawer selector = new ColorSelectorDrawer();
			selector.EnableAlpha = false;

			if (this.Value == null)
				this.Value = new CmColor(1, 1, 1);

			selector.Value = new Color4((CmColor)this.Value);

			selector.ValueChanged += (v) =>
			{
				this.Value = v.Color;
			};

			await viewService.ShowDrawer(selector, this.DisplayName);
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
}
