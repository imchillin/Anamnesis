// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using Anamnesis;
	using PropertyChanged;
	using Color = Anamnesis.Color;
	using Vector = Anamnesis.Vector;

	/// <summary>
	/// Interaction logic for ExtendedAppearanceEditor.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ExtendedAppearanceEditor : UserControl
	{
		private IMemory<Color> skinColorMem;
		private IMemory<Color> skinGlowMem;
		private IMemory<Color> leftEyeColorMem;
		private IMemory<Color> rightEyeColorMem;
		private IMemory<Color> limbalRingColorMem;
		private IMemory<Color> hairTintColorMem;
		private IMemory<Color> hairGlowColorMem;
		private IMemory<Color> highlightTintColorMem;
		private IMemory<Color> lipTintMem;
		private IMemory<float> lipGlossMem;
		private IMemory<Color> mainHandTintMem;
		private IMemory<Vector> mainHandScaleMem;
		private IMemory<Color> offHandTintMem;
		private IMemory<Vector> offHandScaleMem;

		private Color4? lipTint;

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

		public Color4? LipTint
		{
			get
			{
				return this.lipTint;
			}

			set
			{
				this.lipTint = value;

				if (value == null)
					return;

				Color4 v = (Color4)value;

				this.lipTintMem.Value = v.Color;
				this.lipGlossMem.Value = v.A;
			}
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.lipTintMem?.Dispose();
			this.lipGlossMem?.Dispose();
			this.skinColorMem?.Dispose();
			this.skinGlowMem?.Dispose();
			this.leftEyeColorMem?.Dispose();
			this.rightEyeColorMem?.Dispose();
			this.limbalRingColorMem?.Dispose();
			this.hairTintColorMem?.Dispose();
			this.hairGlowColorMem?.Dispose();
			this.highlightTintColorMem?.Dispose();

			this.mainHandTintMem?.Dispose();
			this.mainHandScaleMem?.Dispose();
			this.offHandTintMem?.Dispose();
			this.offHandScaleMem?.Dispose();

			Actor actor = this.DataContext as Actor;

			if (actor == null)
				return;

			this.skinColorMem = actor.GetMemory(Offsets.Main.SkinColor);
			this.skinColorMem.Bind(this, nameof(this.SkinTint));
			this.skinGlowMem = actor.GetMemory(Offsets.Main.SkinGloss);
			this.skinGlowMem.Bind(this, nameof(this.SkinGlow));
			this.leftEyeColorMem = actor.GetMemory(Offsets.Main.LeftEyeColor);
			this.leftEyeColorMem.Bind(this, nameof(this.LeftEyeColor));
			this.rightEyeColorMem = actor.GetMemory(Offsets.Main.RightEyeColor);
			this.rightEyeColorMem.Bind(this, nameof(this.RightEyeColor));
			this.limbalRingColorMem = actor.GetMemory(Offsets.Main.LimbalColor);
			this.limbalRingColorMem.Bind(this, nameof(this.LimbalRingColor));
			this.hairTintColorMem = actor.GetMemory(Offsets.Main.HairColor);
			this.hairTintColorMem.Bind(this, nameof(this.HairTint));
			this.hairGlowColorMem = actor.GetMemory(Offsets.Main.HairGloss);
			this.hairGlowColorMem.Bind(this, nameof(this.HairGlow));
			this.highlightTintColorMem = actor.GetMemory(Offsets.Main.HairHiglight);
			this.highlightTintColorMem.Bind(this, nameof(this.HighlightTint));
			this.lipTintMem = actor.GetMemory(Offsets.Main.MouthColor);
			this.lipGlossMem = actor.GetMemory(Offsets.Main.MouthGloss);
			this.lipTintMem.ValueChanged += this.LipTintMem_ValueChanged;
			this.lipGlossMem.ValueChanged += this.LipGlossMem_ValueChanged;

			this.mainHandTintMem = actor.GetMemory(Offsets.Main.MainHandColor);
			this.mainHandTintMem.Bind(this, nameof(this.MainHandTint));
			this.mainHandScaleMem = actor.GetMemory(Offsets.Main.MainHandScale);
			this.mainHandScaleMem.Bind(this, nameof(this.MainHandScale));
			this.offHandTintMem = actor.GetMemory(Offsets.Main.OffhandColor);
			this.offHandTintMem.Bind(this, nameof(this.OffHandTint));
			this.offHandScaleMem = actor.GetMemory(Offsets.Main.OffhandScale);
			this.offHandScaleMem.Bind(this, nameof(this.OffHandScale));
		}

		private void LipTintMem_ValueChanged(object sender, object value)
		{
			Color4 c = default;
			c.Color = this.lipTintMem.Value;
			c.A = this.lipGlossMem.Value;
			this.LipTint = c;
		}

		private void LipGlossMem_ValueChanged(object sender, object value)
		{
			Color4 c = default;
			c.Color = this.lipTintMem.Value;
			c.A = this.lipGlossMem.Value;
			this.LipTint = c;
		}

		private void OnMainHandZeroScaleClick(object sender, RoutedEventArgs e)
		{
			this.mainHandScaleMem.Value = Vector.Zero;
		}

		private void OnMainHandOneScaleClick(object sender, RoutedEventArgs e)
		{
			this.mainHandScaleMem.Value = Vector.One;
		}

		private void OnOffHandZeroScaleClick(object sender, RoutedEventArgs e)
		{
			this.offHandScaleMem.Value = Vector.Zero;
		}

		private void OnOffHandOneScaleClick(object sender, RoutedEventArgs e)
		{
			this.offHandScaleMem.Value = Vector.One;
		}
	}
}
