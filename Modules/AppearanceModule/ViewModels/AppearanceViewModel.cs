// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.ViewModels
{
	using System;
	using System.ComponentModel;
	using PropertyChanged;

	public class AppearanceViewModel : IDisposable, INotifyPropertyChanged
	{
		public Appearance Appearance;
		private IMemory<Appearance> appearanceMem;

		private readonly Actor selection;

		public AppearanceViewModel(Actor selection)
		{
			this.appearanceMem = selection.GetMemory(Offsets.Main.ActorAppearance);
			this.Appearance = this.appearanceMem.Value;
			this.selection = selection;

			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Appearance.Races Race
		{
			get => this.Appearance.Race;
			set => this.Appearance.Race = value;
		}

		public byte FacePaintColor
		{
			get => this.Appearance.FacePaintColor;
			set => this.Appearance.FacePaintColor = value;
		}

		public byte FacePaint
		{
			get => this.Appearance.FacePaint;
			set => this.Appearance.FacePaint = value;
		}

		public byte Bust
		{
			get => this.Appearance.Bust;
			set => this.Appearance.Bust = value;
		}

		public byte TailEarsType
		{
			get => this.Appearance.TailEarsType;
			set => this.Appearance.TailEarsType = value;
		}

		public byte EarMuscleTailSize
		{
			get => this.Appearance.EarMuscleTailSize;
			set => this.Appearance.EarMuscleTailSize = value;
		}

		public byte LipsToneFurPattern
		{
			get => this.Appearance.LipsToneFurPattern;
			set => this.Appearance.LipsToneFurPattern = value;
		}

		public byte Mouth
		{
			get => this.Appearance.Mouth;
			set => this.Appearance.Mouth = value;
		}

		public byte Jaw
		{
			get => this.Appearance.Jaw;
			set => this.Appearance.Jaw = value;
		}

		public byte Nose
		{
			get => this.Appearance.Nose;
			set => this.Appearance.Nose = value;
		}

		public byte Eyes
		{
			get => this.Appearance.Eyes;
			set => this.Appearance.Eyes = value;
		}

		public byte LEyeColor
		{
			get => this.Appearance.LEyeColor;
			set => this.Appearance.LEyeColor = value;
		}

		public byte LimbalEyes
		{
			get => this.Appearance.LimbalEyes;
			set => this.Appearance.LimbalEyes = value;
		}

		public byte Eyebrows
		{
			get => this.Appearance.Eyebrows;
			set => this.Appearance.Eyebrows = value;
		}

		public byte Highlights
		{
			get => this.Appearance.Highlights;
			set => this.Appearance.Highlights = value;
		}

		public byte HairTone
		{
			get => this.Appearance.HairTone;
			set => this.Appearance.HairTone = value;
		}

		public byte REyeColor
		{
			get => this.Appearance.REyeColor;
			set => this.Appearance.REyeColor = value;
		}

		public byte Skintone
		{
			get => this.Appearance.Skintone;
			set => this.Appearance.Skintone = value;
		}

		public bool EnableHighlights
		{
			get => this.Appearance.EnableHighlights;
			set => this.Appearance.EnableHighlights = value;
		}

		public byte Hair
		{
			get => this.Appearance.Hair;
			set => this.Appearance.Hair = value;
		}

		public byte Head
		{
			get => this.Appearance.Head;
			set => this.Appearance.Head = value;
		}

		public Appearance.Tribes Tribe
		{
			get => this.Appearance.Tribe;
			set => this.Appearance.Tribe = value;
		}

		public byte Height
		{
			get => this.Appearance.Height;
			set => this.Appearance.Height = value;
		}

		public Appearance.Ages Age
		{
			get => this.Appearance.Age;
			set => this.Appearance.Age = value;
		}

		public Appearance.Genders Gender
		{
			get => this.Appearance.Gender;
			set => this.Appearance.Gender = value;
		}

		public Appearance.FacialFeature FacialFeatures
		{
			get => this.Appearance.FacialFeatures;
			set => this.Appearance.FacialFeatures = value;
		}

		public void Dispose()
		{
			this.appearanceMem?.Dispose();
			this.appearanceMem = null;
		}

		[SuppressPropertyChangedWarnings]
		private void OnSelfPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.appearanceMem == null || this.selection == null)
				return;

			this.appearanceMem.Value = this.Appearance;
			this.selection.ActorRefresh();
		}
	}
}
