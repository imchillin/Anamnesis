// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using Anamnesis;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using ConceptMatrix.WpfStyles.Drawers;
	using PropertyChanged;

	using Color = Anamnesis.Color;
	using WpfColor = System.Windows.Media.Color;

	/// <summary>
	/// Interaction logic for ColorEditor.xaml.
	/// </summary>
	public partial class ColorEditor : UserControl, INotifyPropertyChanged
	{
		public static readonly IBind<Color> ValueDp = Binder.Register<Color, ColorEditor>(nameof(Value), OnValueChanged);

		public ColorEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[AlsoNotifyFor(nameof(ColorEditor.R), nameof(ColorEditor.G), nameof(ColorEditor.B))]
		public Color Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		[AlsoNotifyFor(nameof(ColorEditor.Value))]
		[DependsOn(nameof(ColorEditor.Value))]
		public float R
		{
			get
			{
				return this.Value.R;
			}

			set
			{
				this.Value = new Color(value, this.G, this.B);
				this.UpdatePreview();
			}
		}

		[AlsoNotifyFor(nameof(ColorEditor.Value))]
		[DependsOn(nameof(ColorEditor.Value))]
		public float G
		{
			get
			{
				return this.Value.G;
			}

			set
			{
				this.Value = new Color(this.R, value, this.B);
				this.UpdatePreview();
			}
		}

		[AlsoNotifyFor(nameof(ColorEditor.Value))]
		[DependsOn(nameof(ColorEditor.Value))]
		public float B
		{
			get
			{
				return this.Value.B;
			}

			set
			{
				this.Value = new Color(this.R, this.G, value);
				this.UpdatePreview();
			}
		}

		private static double Clamp(double v)
		{
			v = Math.Min(v, 1);
			v = Math.Max(v, 0);
			return v;
		}

		[SuppressPropertyChangedWarnings]
		private static void OnValueChanged(ColorEditor sender, Color v)
		{
			sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ColorEditor.R)));
			sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ColorEditor.G)));
			sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ColorEditor.B)));

			sender.UpdatePreview();
		}

		[SuppressPropertyChangedWarnings]
		private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(ColorEditor.Value)));
			this.UpdatePreview();
		}

		private void UpdatePreview()
		{
			WpfColor c = default;
			c.R = (byte)(Clamp(this.Value.R) * 255);
			c.G = (byte)(Clamp(this.Value.G) * 255);
			c.B = (byte)(Clamp(this.Value.B) * 255);
			c.A = 255;

			this.Preview.Background = new SolidColorBrush(c);
		}

		private async void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();

			ColorSelectorDrawer selector = new ColorSelectorDrawer();
			selector.Value = new Color4(this.Value);
			await viewService.ShowDrawer(selector, "Color");
			this.Value = selector.Value.Color;
		}
	}
}
