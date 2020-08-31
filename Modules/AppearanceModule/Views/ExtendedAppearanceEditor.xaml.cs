// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Views
{
	using System;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using PropertyChanged;

	using Color = Anamnesis.Memory.Color;
	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for ExtendedAppearanceEditor.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ExtendedAppearanceEditor : UserControl
	{
		public ExtendedAppearanceEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public Color SkinTint { get; set; }
		public Color SkinGlow { get; set; }
		public Color LeftEyeColor { get; set; }
		public Color RightEyeColor { get; set; }
		public Color LimbalRingColor { get; set; }
		public Color HairTint { get; set; }
		public Color HairGlow { get; set; }
		public Color HighlightTint { get; set; }
		public Color MainHandTint { get; set; }
		public Vector MainHandScale { get; set; }
		public Color OffHandTint { get; set; }
		public Vector OffHandScale { get; set; }
		public Color4 LipTint { get; set; }
		public float Transparency { get; set; }
		public Vector BustScale { get; set; }
		public float FeatureScale { get; set; }

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
