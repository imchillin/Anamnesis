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

		private IMemory<Appearance> appearanceMem;
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

		private HashSet<string> lockedFields = new HashSet<string>();
		private bool lockChanged = false;

		private Appearance lastAppearance;

		public ExtendedAppearanceEditor()
		{
			this.InitializeComponent();

			this.refreshService = Services.Get<IActorRefreshService>();

			this.ContentArea.DataContext = this;

			this.PropertyChanged += this.OnSelfPropertyChanged;
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
			this.appearanceMem?.Dispose();

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

			this.actor = this.DataContext as Actor;

			this.OnWeaponsChanged();

			if (this.actor == null)
				return;

			this.appearanceMem = this.actor.GetMemory(Offsets.Main.ActorAppearance);
			this.appearanceMem.ValueChanged += this.AppearanceMem_ValueChanged;
			this.lastAppearance = this.appearanceMem.Value;

			this.skinColorMem = this.actor.GetMemory(Offsets.Main.SkinColor);
			this.skinColorMem.ValueChanged += this.SkinValueChanged;
			this.skinGlowMem = this.actor.GetMemory(Offsets.Main.SkinGloss);
			this.skinGlowMem.ValueChanged += this.SkinValueChanged;
			this.leftEyeColorMem = this.actor.GetMemory(Offsets.Main.LeftEyeColor);
			this.leftEyeColorMem.ValueChanged += this.LeftEyeColorValueChanged;
			this.rightEyeColorMem = this.actor.GetMemory(Offsets.Main.RightEyeColor);
			this.rightEyeColorMem.ValueChanged += this.RightEyeColorValueChanged;
			this.limbalRingColorMem = this.actor.GetMemory(Offsets.Main.LimbalColor);
			this.limbalRingColorMem.ValueChanged += this.LimbalRingColorValueChanged;
			this.hairTintColorMem = this.actor.GetMemory(Offsets.Main.HairColor);
			this.hairTintColorMem.ValueChanged += this.HairValueChanged;
			this.hairGlowColorMem = this.actor.GetMemory(Offsets.Main.HairGloss);
			this.hairGlowColorMem.ValueChanged += this.HairValueChanged;
			this.highlightTintColorMem = this.actor.GetMemory(Offsets.Main.HairHiglight);
			this.highlightTintColorMem.ValueChanged += this.HairValueChanged;
			this.lipTintMem = this.actor.GetMemory(Offsets.Main.MouthColor);
			this.lipGlossMem = this.actor.GetMemory(Offsets.Main.MouthGloss);
			this.lipTintMem.ValueChanged += this.LipValueChanged;
			this.lipGlossMem.ValueChanged += this.LipValueChanged;

			this.HairTint = this.hairTintColorMem.Value;
			this.HairGlow = this.hairGlowColorMem.Value;
			this.HighlightTint = this.highlightTintColorMem.Value;
			this.LimbalRingColor = this.limbalRingColorMem.Value;
			this.RightEyeColor = this.rightEyeColorMem.Value;
			this.LeftEyeColor = this.leftEyeColorMem.Value;
			this.SkinTint = this.skinColorMem.Value;
			this.SkinGlow = this.skinGlowMem.Value;

			Color4 c = default;
			c.Color = this.lipTintMem.Value;
			c.A = this.lipGlossMem.Value;
			this.LipTint = c;

			this.mainHandMem = this.actor.GetMemory(Offsets.Main.MainHand);
			this.mainHandMem.ValueChanged += this.OnWeaponsChanged;
			this.offHandMem = this.actor.GetMemory(Offsets.Main.OffHand);
			this.offHandMem.ValueChanged += this.OnWeaponsChanged;

			this.OnWeaponsChanged();
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
			this.HasMainHand = this.mainHandMem != null && this.mainHandMem.Value.Base != 0;
			if (this.HasMainHand)
			{
				this.mainHandTintMem = this.actor.GetMemory(Offsets.Main.MainHandColor);
				this.mainHandTintMem.ValueChanged += this.MainHandTintMem_ValueChanged;
				this.mainHandScaleMem = this.actor.GetMemory(Offsets.Main.MainHandScale);
				this.mainHandScaleMem.ValueChanged += this.MainHandScaleMem_ValueChanged;

				this.MainHandScale = Vector.One;
				this.MainHandTint = Color.White;
			}

			// do we have an off hand?
			this.HasOffHand = this.offHandMem != null && this.offHandMem.Value.Base != 0;
			if (this.HasOffHand)
			{
				this.offHandTintMem = this.actor.GetMemory(Offsets.Main.OffhandColor);
				this.offHandTintMem.ValueChanged += this.OffHandTintMem_ValueChanged;
				this.offHandScaleMem = this.actor.GetMemory(Offsets.Main.OffhandScale);
				this.offHandScaleMem.ValueChanged += this.OffHandScaleMem_ValueChanged;

				this.OffHandScale = Vector.One;
				this.OffHandTint = Color.White;
			}
		}

		private void AppearanceMem_ValueChanged(object sender, object value)
		{
			Appearance newAppearance = this.appearanceMem.Value;

			// Determine what values were changed
			foreach (FieldInfo field in typeof(Appearance).GetFields())
			{
				object newValue = field.GetValue(newAppearance);
				object oldValue = field.GetValue(this.lastAppearance);

				if (!newValue.Equals(oldValue))
				{
					string fieldName = field.Name;
					this.lockedFields.Add(fieldName);
				}
			}

			this.lastAppearance = newAppearance;
		}

		private async void HairValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains(nameof(Appearance.HairTone)))
			{
				this.HairTint = this.hairTintColorMem.Value;
				this.HairGlow = this.hairGlowColorMem.Value;
				this.HighlightTint = this.highlightTintColorMem.Value;
				this.lockedFields.Remove(nameof(Appearance.HairTone));
			}
			else
			{
				await this.AwaitRefresh();

				this.hairTintColorMem.Value = this.HairTint;
				this.hairGlowColorMem.Value = this.HairGlow;
				this.highlightTintColorMem.Value = this.HighlightTint;
			}

			this.lockChanged = false;
		}

		private async void LimbalRingColorValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains(nameof(Appearance.LimbalEyes)))
			{
				this.LimbalRingColor = this.limbalRingColorMem.Value;
				this.lockedFields.Remove(nameof(Appearance.LimbalEyes));
			}
			else
			{
				await this.AwaitRefresh();

				this.limbalRingColorMem.Value = this.LimbalRingColor;
			}

			this.lockChanged = false;
		}

		private async void RightEyeColorValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains(nameof(Appearance.REyeColor)))
			{
				this.RightEyeColor = this.rightEyeColorMem.Value;
				this.lockedFields.Remove(nameof(Appearance.REyeColor));
			}
			else
			{
				await this.AwaitRefresh();

				this.rightEyeColorMem.Value = this.RightEyeColor;
			}

			this.lockChanged = false;
		}

		private async void LeftEyeColorValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains(nameof(Appearance.LEyeColor)))
			{
				this.LeftEyeColor = this.leftEyeColorMem.Value;
				this.lockedFields.Remove(nameof(Appearance.LEyeColor));
			}
			else
			{
				await this.AwaitRefresh();

				this.leftEyeColorMem.Value = this.LeftEyeColor;
			}

			this.lockChanged = false;
		}

		private async void SkinValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains(nameof(Appearance.Skintone)))
			{
				this.SkinTint = this.skinColorMem.Value;
				this.SkinGlow = this.skinGlowMem.Value;
				this.lockedFields.Remove(nameof(Appearance.Skintone));
			}
			else
			{
				await this.AwaitRefresh();

				this.skinColorMem.Value = this.SkinTint;
				this.skinGlowMem.Value = this.SkinGlow;
			}

			this.lockChanged = false;
		}

		private async void LipValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains(nameof(Appearance.LipsToneFurPattern)))
			{
				Color4 c = default;
				c.Color = this.lipTintMem.Value;
				c.A = this.lipGlossMem.Value;
				this.LipTint = c;

				this.lockedFields.Remove(nameof(Appearance.LipsToneFurPattern));
			}
			else
			{
				await this.AwaitRefresh();

				Color4 c = (Color4)this.LipTint;
				this.lipTintMem.Value = c.Color;
				this.lipGlossMem.Value = c.A;
			}

			this.lockChanged = false;
		}

		private async void OffHandScaleMem_ValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains("??"))
			{
				////this.lockedFields.Remove("??");
			}
			else
			{
				await this.AwaitRefresh();

				this.offHandScaleMem.Value = this.OffHandScale;
			}

			this.lockChanged = false;
		}

		private async void OffHandTintMem_ValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains("??"))
			{
				////this.lockedFields.Remove("??");
			}
			else
			{
				await this.AwaitRefresh();

				this.offHandTintMem.Value = this.OffHandTint;
			}

			this.lockChanged = false;
		}

		private async void MainHandScaleMem_ValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains("??"))
			{
				////this.lockedFields.Remove("??");
			}
			else
			{
				await this.AwaitRefresh();

				this.mainHandScaleMem.Value = this.MainHandScale;
			}

			this.lockChanged = false;
		}

		private async void MainHandTintMem_ValueChanged(object sender, object value)
		{
			this.lockChanged = true;

			if (this.lockedFields.Contains("??"))
			{
				////this.lockedFields.Remove("??");
			}
			else
			{
				await this.AwaitRefresh();

				this.mainHandTintMem.Value = this.MainHandTint;
			}

			this.lockChanged = false;
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

		private async Task AwaitRefresh()
		{
			while (this.refreshService.IsRefreshing)
			{
				await Task.Delay(10);
			}
		}

		private void OnSelfPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.lockChanged)
				return;

			if (e.PropertyName == nameof(this.LipTint))
			{
				Color4 c = (Color4)this.LipTint;
				this.lipTintMem.Value = c.Color;
				this.lipGlossMem.Value = c.A;
			}
			else if (e.PropertyName == nameof(this.SkinTint))
			{
				this.skinColorMem.Value = this.SkinTint;
			}
			else if (e.PropertyName == nameof(this.SkinGlow))
			{
				this.skinGlowMem.Value = this.SkinGlow;
			}
			else if (e.PropertyName == nameof(this.LeftEyeColor))
			{
				this.leftEyeColorMem.Value = this.LeftEyeColor;
			}
			else if (e.PropertyName == nameof(this.RightEyeColor))
			{
				this.rightEyeColorMem.Value = this.RightEyeColor;
			}
			else if (e.PropertyName == nameof(this.LimbalRingColor))
			{
				this.limbalRingColorMem.Value = this.LimbalRingColor;
			}
			else if (e.PropertyName == nameof(this.HairTint))
			{
				this.hairTintColorMem.Value = this.HairTint;
			}
			else if (e.PropertyName == nameof(this.HairGlow))
			{
				this.hairGlowColorMem.Value = this.HairGlow;
			}
			else if (e.PropertyName == nameof(this.HighlightTint))
			{
				this.highlightTintColorMem.Value = this.HighlightTint;
			}
			else if (e.PropertyName == nameof(this.MainHandScale))
			{
				this.mainHandScaleMem.Value = this.MainHandScale;
			}
			else if (e.PropertyName == nameof(this.MainHandTint))
			{
				this.mainHandTintMem.Value = this.MainHandTint;
			}
			else if (e.PropertyName == nameof(this.OffHandScale))
			{
				this.offHandScaleMem.Value = this.OffHandScale;
			}
			else if (e.PropertyName == nameof(this.OffHandTint))
			{
				this.offHandTintMem.Value = this.OffHandTint;
			}
		}
	}
}
