// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.ViewModels
{
	using System;
	using Anamnesis;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ExtendedAppearanceViewModel : IDisposable
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

		private Color4? lipTint;
		private Color? skinTint;
		private Color? skinGlow;
		private Color? leftEyeColor;
		private Color? rightEyeColor;
		private Color? limbalRingColor;
		private Color? hairTint;
		private Color? hairGlow;
		private Color? highlightTint;

		public ExtendedAppearanceViewModel(Actor actor)
		{
			this.skinColorMem = actor.GetMemory(Offsets.Main.SkinColor);
			this.skinColorMem.ValueChanged += (_, v) => this.SkinTint = (Color?)v;

			this.skinGlowMem = actor.GetMemory(Offsets.Main.SkinGloss);
			this.skinGlowMem.ValueChanged += (_, v) => this.skinGlow = (Color?)v;

			this.leftEyeColorMem = actor.GetMemory(Offsets.Main.LeftEyeColor);
			this.leftEyeColorMem.ValueChanged += (_, v) => this.skinGlow = (Color?)v;

			this.rightEyeColorMem = actor.GetMemory(Offsets.Main.RightEyeColor);
			this.rightEyeColorMem.ValueChanged += (_, v) => this.skinGlow = (Color?)v;

			this.limbalRingColorMem = actor.GetMemory(Offsets.Main.LimbalColor);
			this.limbalRingColorMem.ValueChanged += (_, v) => this.skinGlow = (Color?)v;

			this.hairTintColorMem = actor.GetMemory(Offsets.Main.HairColor);
			this.hairTintColorMem.ValueChanged += (_, v) => this.skinGlow = (Color?)v;

			this.hairGlowColorMem = actor.GetMemory(Offsets.Main.HairGloss);
			this.hairGlowColorMem.ValueChanged += (_, v) => this.skinGlow = (Color?)v;

			this.highlightTintColorMem = actor.GetMemory(Offsets.Main.HairHiglight);
			this.highlightTintColorMem.ValueChanged += (_, v) => this.skinGlow = (Color?)v;

			this.lipTintMem = actor.GetMemory(Offsets.Main.MouthColor);
			this.lipTintMem.ValueChanged += this.LipTintMem_ValueChanged;

			this.lipGlossMem = actor.GetMemory(Offsets.Main.MouthGloss);
			this.lipGlossMem.ValueChanged += this.LipGlossMem_ValueChanged;
		}

		public Color? SkinTint
		{
			get
			{
				return this.skinTint;
			}
			set
			{
				this.skinTint = value;

				if (value != null)
				{
					this.skinColorMem.Value = (Color)value;
				}
			}
		}

		public Color? SkinGlow
		{
			get
			{
				return this.skinGlow;
			}
			set
			{
				this.skinGlow = value;

				if (value != null)
				{
					this.skinGlowMem.Value = (Color)value;
				}
			}
		}

		public Color? LeftEyeColor
		{
			get
			{
				return this.leftEyeColor;
			}
			set
			{
				this.leftEyeColor = value;

				if (value != null)
				{
					this.leftEyeColorMem.Value = (Color)value;
				}
			}
		}

		public Color? RightEyeColor
		{
			get
			{
				return this.rightEyeColor;
			}
			set
			{
				this.rightEyeColor = value;

				if (value != null)
				{
					this.rightEyeColorMem.Value = (Color)value;
				}
			}
		}

		public Color? LimbalRingColor
		{
			get
			{
				return this.limbalRingColor;
			}
			set
			{
				this.limbalRingColor = value;

				if (value != null)
				{
					this.limbalRingColorMem.Value = (Color)value;
				}
			}
		}

		public Color? HairTint
		{
			get
			{
				return this.hairTint;
			}
			set
			{
				this.hairTint = value;

				if (value != null)
				{
					this.hairTintColorMem.Value = (Color)value;
				}
			}
		}

		public Color? HairGlow
		{
			get
			{
				return this.hairGlow;
			}
			set
			{
				this.hairGlow = value;

				if (value != null)
				{
					this.hairGlowColorMem.Value = (Color)value;
				}
			}
		}

		public Color? HighlightTint
		{
			get
			{
				return this.highlightTint;
			}
			set
			{
				this.highlightTint = value;

				if (value != null)
				{
					this.highlightTintColorMem.Value = (Color)value;
				}
			}
		}

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

		public void Dispose()
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
	}
}
