// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
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
	using Anamnesis;
	using PropertyChanged;
	using Color = Anamnesis.Color;
	using Vector = Anamnesis.Vector;

	/// <summary>
	/// Interaction logic for ExtendedAppearanceEditor.xaml.
	/// </summary>
	// Boy I hate this class.
	// theres a bunch of weird property bindings set up here to handle the case where a user wants to
	// reset an extended appearance value by setting its underlying value in the appearance editor, but since
	// we don't have any implicit links between the two (e.g: Appearance.LipTone and LipTint + LipGloss) we're just
	// going to look at the property changed and memory changed events to figure out if we want to accept the memory changes
	// or overwrite them.
	public partial class ExtendedAppearanceEditor : UserControl, INotifyPropertyChanged
	{
		private Actor actor;

		private IActorRefreshService refreshService;

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

		private IMemory<Weapon> mainHandMem;
		private IMemory<Color> mainHandTintMem;
		private IMemory<Vector> mainHandScaleMem;
		private IMemory<Weapon> offHandMem;
		private IMemory<Color> offHandTintMem;
		private IMemory<Vector> offHandScaleMem;

		public ExtendedAppearanceEditor()
		{
			this.InitializeComponent();

			this.refreshService = Services.Get<IActorRefreshService>();

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

		public bool HasMainHand { get; set; }
		public bool HasOffHand { get; set; }

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.actor = this.DataContext as Actor;

			this.OnActorRetargetBegin(this.actor);

			if (this.actor == null)
				return;

			this.actor.ActorRetargetBegin += this.OnActorRetargetBegin;
			this.actor.ActorRetargetComplete += this.OnActorRetargetComplete;
			this.OnActorRetargetComplete(this.actor);
		}

		private void OnActorRetargetBegin(Actor actor)
		{
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
		}

		private void OnActorRetargetComplete(Actor actor = null)
		{
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
			this.lipTintMem.ValueChanged += this.LipValueChanged;
			this.lipGlossMem.ValueChanged += this.LipValueChanged;

			this.mainHandMem = this.actor.GetMemory(Offsets.Main.MainHand);
			this.mainHandMem.ValueChanged += this.OnWeaponsChanged;
			this.offHandMem = this.actor.GetMemory(Offsets.Main.OffHand);
			this.offHandMem.ValueChanged += this.OnWeaponsChanged;

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

		private void LipValueChanged(object sender, object value)
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
