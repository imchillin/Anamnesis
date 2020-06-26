// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.ViewModels
{
	using System;
	using System.ComponentModel;
	using System.Reflection;
	using System.Windows;
	using Anamnesis;
	using PropertyChanged;

	public class AppearanceViewModel : IDisposable, INotifyPropertyChanged
	{
		private readonly Actor selection;
		private IMemory<Appearance> appearanceMem;
		private bool lockChangedEvent = false;

		public AppearanceViewModel(Actor selection)
		{
			this.appearanceMem = selection.GetMemory(Offsets.Main.ActorAppearance);
			this.appearanceMem.ValueChanged += this.OnMemValueChanged;
			this.selection = selection;

			this.PropertyChanged += this.OnPropertyChanged;

			this.OnMemValueChanged(null, null);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Appearance.Races Race { get; set; }
		public byte FacePaintColor { get; set; }
		public byte FacePaint { get; set; }
		public byte Bust { get; set; }
		public byte TailEarsType { get; set; }
		public byte EarMuscleTailSize { get; set; }
		public byte LipsToneFurPattern { get; set; }
		public byte Mouth { get; set; }
		public byte Jaw { get; set; }
		public byte Nose { get; set; }
		public byte Eyes { get; set; }
		public byte LEyeColor { get; set; }
		public byte LimbalEyes { get; set; }
		public byte Eyebrows { get; set; }
		public byte Highlights { get; set; }
		public byte HairTone { get; set; }
		public byte REyeColor { get; set; }
		public byte Skintone { get; set; }
		public bool EnableHighlights { get; set; }
		public byte Hair { get; set; }
		public byte Head { get; set; }
		public Appearance.Tribes Tribe { get; set; }
		public byte Height { get; set; }
		public Appearance.Ages Age { get; set; }
		public Appearance.Genders Gender { get; set; }
		public Appearance.FacialFeature FacialFeatures { get; set; }

		public void Dispose()
		{
			this.appearanceMem?.Dispose();
			this.appearanceMem = null;
		}

		private void OnMemValueChanged(object sender, object value)
		{
			this.lockChangedEvent = true;

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.RaiseIfChanged(nameof(this.Race));
				this.RaiseIfChanged(nameof(this.Tribe));

				this.RaiseIfChanged(nameof(this.Gender));
				this.RaiseIfChanged(nameof(this.Age));
				this.RaiseIfChanged(nameof(this.Height));
				this.RaiseIfChanged(nameof(this.Head));
				this.RaiseIfChanged(nameof(this.Hair));
				this.RaiseIfChanged(nameof(this.EnableHighlights));
				this.RaiseIfChanged(nameof(this.Skintone));
				this.RaiseIfChanged(nameof(this.REyeColor));
				this.RaiseIfChanged(nameof(this.HairTone));
				this.RaiseIfChanged(nameof(this.Highlights));
				this.RaiseIfChanged(nameof(this.FacialFeatures));
				this.RaiseIfChanged(nameof(this.LimbalEyes));
				this.RaiseIfChanged(nameof(this.Eyebrows));
				this.RaiseIfChanged(nameof(this.LEyeColor));
				this.RaiseIfChanged(nameof(this.Eyes));
				this.RaiseIfChanged(nameof(this.Nose));
				this.RaiseIfChanged(nameof(this.Jaw));
				this.RaiseIfChanged(nameof(this.Mouth));
				this.RaiseIfChanged(nameof(this.LipsToneFurPattern));
				this.RaiseIfChanged(nameof(this.EarMuscleTailSize));
				this.RaiseIfChanged(nameof(this.TailEarsType));
				this.RaiseIfChanged(nameof(this.Bust));
				this.RaiseIfChanged(nameof(this.FacePaint));
				this.RaiseIfChanged(nameof(this.FacePaintColor));
			});

			this.lockChangedEvent = false;
		}

		private void RaiseIfChanged(string propertyName)
		{
			if (this.appearanceMem == null || !this.appearanceMem.Active)
				return;

			FieldInfo field = typeof(Appearance).GetField(propertyName);
			PropertyInfo property = typeof(AppearanceViewModel).GetProperty(propertyName);

			object oldValue = property.GetValue(this);
			object newValue = field.GetValue(this.appearanceMem.Value);

			if (oldValue.Equals(newValue))
				return;

			property.SetValue(this, newValue);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.lockChangedEvent)
				return;

			Appearance appearance = this.appearanceMem.Value;
			appearance.Race = this.Race;
			appearance.Gender = this.Gender;
			appearance.Age = this.Age;
			appearance.Height = this.Height;
			appearance.Tribe = this.Tribe;
			appearance.Head = this.Head;
			appearance.Hair = this.Hair;
			appearance.EnableHighlights = this.EnableHighlights;
			appearance.Skintone = this.Skintone;
			appearance.REyeColor = this.REyeColor;
			appearance.HairTone = this.HairTone;
			appearance.Highlights = this.Highlights;
			appearance.FacialFeatures = this.FacialFeatures;
			appearance.LimbalEyes = this.LimbalEyes;
			appearance.Eyebrows = this.Eyebrows;
			appearance.LEyeColor = this.LEyeColor;
			appearance.Eyes = this.Eyes;
			appearance.Nose = this.Nose;
			appearance.Jaw = this.Jaw;
			appearance.Mouth = this.Mouth;
			appearance.LipsToneFurPattern = this.LipsToneFurPattern;
			appearance.EarMuscleTailSize = this.EarMuscleTailSize;
			appearance.TailEarsType = this.TailEarsType;
			appearance.Bust = this.Bust;
			appearance.FacePaint = this.FacePaint;
			appearance.FacePaintColor = this.FacePaintColor;
			this.appearanceMem.Value = appearance;

			this.selection.ActorRefresh();
		}
	}
}
