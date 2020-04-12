// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.ViewModels
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;

	public class AppearanceViewModel : IDisposable, INotifyPropertyChanged
	{
		// how long to wait after a change before calling Apply()
		private const int ApplyDelay = 500;

		private IMemory<Appearance> appearanceMem;
		private Appearance appearance;
		private Selection selection;

		private int applyCountdown = 0;
		private Task applyTask;

		public AppearanceViewModel(Selection selection)
		{
			this.appearanceMem = selection.BaseAddress.GetMemory(Offsets.ActorAppearance);
			this.appearance = this.appearanceMem.Value;
			this.selection = selection;

			this.PropertyChanged += this.OnPropertyChanged;
			this.SubPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
		}

		public event PropertyChangedEventHandler SubPropertyChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		public Appearance.Races Race
		{
			get => this.appearance.Race;
			set => this.appearance.Race = value;
		}

		public byte FacePaintColor
		{
			get => this.appearance.FacePaintColor;
			set => this.appearance.FacePaintColor = value;
		}

		public byte FacePaint
		{
			get => this.appearance.FacePaint;
			set => this.appearance.FacePaint = value;
		}

		public byte Bust
		{
			get => this.appearance.Bust;
			set => this.appearance.Bust = value;
		}

		public byte TailEarsType
		{
			get => this.appearance.TailEarsType;
			set => this.appearance.TailEarsType = value;
		}

		public byte EarMuscleTailSize
		{
			get => this.appearance.EarMuscleTailSize;
			set => this.appearance.EarMuscleTailSize = value;
		}

		public byte LipsToneFurPattern
		{
			get => this.appearance.LipsToneFurPattern;
			set => this.appearance.LipsToneFurPattern = value;
		}

		public byte Lips
		{
			get => this.appearance.Lips;
			set => this.appearance.Lips = value;
		}

		public byte Jaw
		{
			get => this.appearance.Jaw;
			set => this.appearance.Jaw = value;
		}

		public byte Nose
		{
			get => this.appearance.Nose;
			set => this.appearance.Nose = value;
		}

		public byte Eyes
		{
			get => this.appearance.Eyes;
			set => this.appearance.Eyes = value;
		}

		public byte LEyeColor
		{
			get => this.appearance.LEyeColor;
			set => this.appearance.LEyeColor = value;
		}

		public byte LimbalEyes
		{
			get => this.appearance.LimbalEyes;
			set => this.appearance.LimbalEyes = value;
		}

		public byte Eyebrows
		{
			get => this.appearance.Eyebrows;
			set => this.appearance.Eyebrows = value;
		}

		public byte HighlightTone
		{
			get => this.appearance.HighlightTone;
			set => this.appearance.HighlightTone = value;
		}

		public byte HairTone
		{
			get => this.appearance.HairTone;
			set => this.appearance.HairTone = value;
		}

		public byte REyeColor
		{
			get => this.appearance.REyeColor;
			set => this.appearance.REyeColor = value;
		}

		public byte Skintone
		{
			get => this.appearance.Skintone;
			set => this.appearance.Skintone = value;
		}

		public byte Highlights
		{
			get => this.appearance.Highlights;
			set => this.appearance.Highlights = value;
		}

		public byte Hair
		{
			get => this.appearance.Hair;
			set => this.appearance.Hair = value;
		}

		public byte Head
		{
			get => this.appearance.Head;
			set => this.appearance.Head = value;
		}

		public Appearance.Tribes Tribe
		{
			get => this.appearance.Tribe;
			set => this.appearance.Tribe = value;
		}

		public byte Height
		{
			get => this.appearance.Height;
			set => this.appearance.Height = value;
		}

		public Appearance.Ages Age
		{
			get => this.appearance.Age;
			set => this.appearance.Age = value;
		}

		public Appearance.Genders Gender
		{
			get => this.appearance.Gender;
			set => this.appearance.Gender = value;
		}

		public byte FacialFeatures
		{
			get => this.appearance.FacialFeatures;
			set => this.appearance.FacialFeatures = value;
		}

		public void Dispose()
		{
			this.appearanceMem.Dispose();
			this.appearanceMem = null;
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.applyCountdown = ApplyDelay;

			if (this.applyTask == null || this.applyTask.IsCompleted)
			{
				this.applyTask = this.ApplyAfterDelay();
			}

			this.SubPropertyChanged?.Invoke(this, e);
		}

		private async Task ApplyAfterDelay()
		{
			while (this.applyCountdown > 0)
			{
				this.applyCountdown -= 100;
				await Task.Delay(100);
			}

			this.Apply();
		}

		private void Apply()
		{
			if (this.appearanceMem == null)
				return;

			this.appearanceMem.Value = this.appearance;
			this.selection.ActorRefresh();
		}
	}
}
