// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using Anamnesis.Memory;
	using PropertyChanged;
	using Color = Anamnesis.Memory.Color;
	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for ExtendedAppearanceEditor.xaml.
	/// </summary>
	// Boy I hate this class.
	// theres a bunch of weird property bindings set up here to handle the case where a user wants to
	// reset an extended appearance value by setting its underlying value in the appearance editor, but since
	// we don't have any implicit links between the two (e.g: Appearance.LipTone and LipTint + LipGloss) we're just
	// going to look at the property changed and memory changed events to figure out if we want to accept the memory changes
	// or overwrite them.
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class ExtendedAppearanceEditor : UserControl, INotifyPropertyChanged
	{
		private Actor actor;

		private IMarshaler<Color> skinColorMem;
		private IMarshaler<Color> skinGlowMem;
		private IMarshaler<Color> leftEyeColorMem;
		private IMarshaler<Color> rightEyeColorMem;
		private IMarshaler<Color> limbalRingColorMem;
		private IMarshaler<Color> hairTintColorMem;
		private IMarshaler<Color> hairGlowColorMem;
		private IMarshaler<Color> highlightTintColorMem;
		private IMarshaler<Color> lipTintMem;
		private IMarshaler<float> lipGlossMem;
		private IMarshaler<Weapon> mainHandMem;
		private IMarshaler<Color> mainHandTintMem;
		private IMarshaler<Vector> mainHandScaleMem;
		private IMarshaler<Weapon> offHandMem;
		private IMarshaler<Color> offHandTintMem;
		private IMarshaler<Vector> offHandScaleMem;
		private IMarshaler<float> transparencyMem;
		private IMarshaler<Vector> bustScaleMem;
		private IMarshaler<float> featureScaleMem;

		public ExtendedAppearanceEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public event PropertyChangedEventHandler PropertyChanged;

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

		public bool HasMainHand { get; set; }
		public bool HasOffHand { get; set; }

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				this.SetActor(this.DataContext as Actor);
				this.IsEnabled = true;
			}
			catch (Exception ex)
			{
				this.IsEnabled = false;
				Log.Write(new Exception("Failed to set actor for extended appearance", ex));
			}

			if (this.actor == null)
				return;

			this.PropertyChanged += this.OnThisPropertyChanged;
		}

		private void OnThisPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(this.LipTint))
			{
				this.lipTintMem.Value = this.LipTint.Color;
				this.lipGlossMem.Value = this.LipTint.A;
			}
		}

		private void SetActor(Actor actor)
		{
			this.actor = actor;

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = false;
			});

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
			this.offHandMem?.Dispose();
			this.mainHandMem?.Dispose();
			this.mainHandTintMem?.Dispose();
			this.mainHandScaleMem?.Dispose();
			this.offHandTintMem?.Dispose();
			this.offHandScaleMem?.Dispose();
			this.transparencyMem?.Dispose();
			this.bustScaleMem?.Dispose();
			this.featureScaleMem?.Dispose();

			if (this.actor == null)
				return;

			if (!actor.IsCustomizable())
				return;

			this.skinColorMem = this.actor.GetMemory(Offsets.Main.SkinColor);
			this.skinColorMem.Bind(this, nameof(this.SkinTint));

			this.skinGlowMem = this.actor.GetMemory(Offsets.Main.SkinGloss);
			this.skinGlowMem.Bind(this, nameof(this.SkinGlow));

			this.leftEyeColorMem = this.actor.GetMemory(Offsets.Main.LeftEyeColor);
			this.leftEyeColorMem.Bind(this, nameof(this.LeftEyeColor));
			this.rightEyeColorMem = this.actor.GetMemory(Offsets.Main.RightEyeColor);
			this.rightEyeColorMem.Bind(this, nameof(this.RightEyeColor));
			this.limbalRingColorMem = this.actor.GetMemory(Offsets.Main.LimbalColor);
			this.limbalRingColorMem.Bind(this, nameof(this.LimbalRingColor));

			this.hairTintColorMem = this.actor.GetMemory(Offsets.Main.HairColor);
			this.hairTintColorMem.Bind(this, nameof(this.HairTint));
			this.hairGlowColorMem = this.actor.GetMemory(Offsets.Main.HairGloss);
			this.hairGlowColorMem.Bind(this, nameof(this.HairGlow));
			this.highlightTintColorMem = this.actor.GetMemory(Offsets.Main.HairHiglight);
			this.highlightTintColorMem.Bind(this, nameof(this.HighlightTint));
			this.lipTintMem = this.actor.GetMemory(Offsets.Main.MouthColor);
			this.lipGlossMem = this.actor.GetMemory(Offsets.Main.MouthGloss);
			this.lipTintMem.ValueChanged += this.LipTintValueChanged;
			this.lipGlossMem.ValueChanged += this.LipGlossValueChanged;

			this.mainHandMem = this.actor.GetMemory(Offsets.Main.MainHand);
			this.mainHandMem.ValueChanged += this.OnWeaponsChanged;
			this.offHandMem = this.actor.GetMemory(Offsets.Main.OffHand);
			this.offHandMem.ValueChanged += this.OnWeaponsChanged;

			this.transparencyMem = actor.GetMemory(Offsets.Main.Transparency);
			this.transparencyMem.Bind(this, nameof(this.Transparency));

			this.bustScaleMem = actor.GetMemory(Offsets.Main.BustScale);
			this.bustScaleMem.Bind(this, nameof(this.BustScale));

			this.featureScaleMem = actor.GetMemory(Offsets.Main.UniqueFeatureScale);
			this.featureScaleMem.Bind(this, nameof(this.FeatureScale));

			this.OnWeaponsChanged();

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = true;
			});
		}

		private void OnWeaponsChanged(object sender = null, object value = null)
		{
			this.mainHandTintMem?.Dispose();
			this.mainHandScaleMem?.Dispose();
			this.offHandTintMem?.Dispose();
			this.offHandScaleMem?.Dispose();

			if (this.actor == null)
				return;

			// do we have a main hand?
			this.HasMainHand = this.mainHandMem != null && this.mainHandMem.Active && this.mainHandMem.Value.Base != 0;
			if (this.HasMainHand)
			{
				this.mainHandTintMem = this.actor.GetMemory(Offsets.Main.MainHandColor);
				this.mainHandTintMem.Bind(this, nameof(this.MainHandTint));
				this.mainHandScaleMem = this.actor.GetMemory(Offsets.Main.MainHandScale);
				this.mainHandScaleMem.Bind(this, nameof(this.MainHandScale));

				this.MainHandScale = Vector.One;
				this.MainHandTint = Color.White;
			}

			// do we have an off hand?
			this.HasOffHand = this.offHandMem != null && this.offHandMem.Active && this.offHandMem.Value.Base != 0;
			if (this.HasOffHand)
			{
				this.offHandTintMem = this.actor.GetMemory(Offsets.Main.OffhandColor);
				this.offHandTintMem.Bind(this, nameof(this.OffHandTint));
				this.offHandScaleMem = this.actor.GetMemory(Offsets.Main.OffhandScale);
				this.offHandScaleMem.Bind(this, nameof(this.OffHandScale));

				this.OffHandScale = Vector.One;
				this.OffHandTint = Color.White;
			}
		}

		private void LipGlossValueChanged(object sender, float value)
		{
			if (!this.lipTintMem.Active || !this.lipGlossMem.Active)
				return;

			Color4 c = default;
			c.Color = this.lipTintMem.Value;
			c.A = this.lipGlossMem.Value;
			this.LipTint = c;
		}

		private void LipTintValueChanged(object sender, Color value)
		{
			if (!this.lipTintMem.Active || !this.lipGlossMem.Active)
				return;

			Color4 c = default;
			c.Color = this.lipTintMem.Value;
			c.A = this.lipGlossMem.Value;
			this.LipTint = c;
		}

		private void OnMainHandZeroScaleClick(object sender, RoutedEventArgs e)
		{
			this.MainHandScale = Vector.Zero;
		}

		private void OnMainHandOneScaleClick(object sender, RoutedEventArgs e)
		{
			this.MainHandScale = Vector.One;
		}

		private void OnOffHandZeroScaleClick(object sender, RoutedEventArgs e)
		{
			this.OffHandScale = Vector.Zero;
		}

		private void OnOffHandOneScaleClick(object sender, RoutedEventArgs e)
		{
			this.OffHandScale = Vector.One;
		}
	}
}
