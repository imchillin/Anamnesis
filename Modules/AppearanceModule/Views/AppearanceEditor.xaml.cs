// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	public partial class AppearanceEditor : UserControl, INotifyPropertyChanged
	{
		private readonly IGameDataService gameDataService;

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

		public AppearanceEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.gameDataService = Services.Get<IGameDataService>();

			this.GenderComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Genders));
			this.RaceComboBox.ItemsSource = this.gameDataService.Races.All;
			this.AgeComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Ages));

			this.PropertyChanged += this.AppearanceEditor_PropertyChanged;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool HasGender { get; set; }
		public bool HasFur { get; set; }
		public bool HasLimbal { get; set; }
		public bool HasTail { get; set; }
		public bool HasEars { get; set; }
		public bool HasMuscles { get; set; }
		public bool CanAge { get; set; }
		public ICharaMakeCustomize Hair { get; set; }

		public IRace Race
		{
			get
			{
				if (this.Appearance == null)
					return null;

				return this.gameDataService.Races.Get((int)this.Appearance.Race);
			}

			set
			{
				if (value == null)
					return;

				if (value.Race == ConceptMatrix.Appearance.Races.Hrothgar)
					this.Appearance.Gender = ConceptMatrix.Appearance.Genders.Masculine;

				if (value.Race == ConceptMatrix.Appearance.Races.Viera)
					this.Appearance.Gender = ConceptMatrix.Appearance.Genders.Feminine;

				this.Appearance.Race = value.Race;
				this.TribeComboBox.ItemsSource = value.Tribes;
				this.Tribe = value.Tribes.First();
			}
		}

		public ITribe Tribe
		{
			get
			{
				if (this.Appearance == null)
					return null;

				return this.gameDataService.Tribes.Get((int)this.Appearance.Tribe);
			}

			set
			{
				if (value == null)
					return;

				this.Appearance.Tribe = value.Tribe;
			}
		}

		public Color? SkinTint { get; set; }
		public Color? SkinGlow { get; set; }
		public Color? LeftEyeColor { get; set; }
		public Color? RightEyeColor { get; set; }
		public Color? LimbalRingColor { get; set; }
		public Color? HairTint { get; set; }
		public Color? HairGlow { get; set; }
		public Color? HighlightTint { get; set; }
		public Color4? LipTint { get; set; }

		public AppearanceViewModel Appearance
		{
			get;
			private set;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as Actor);
		}

		[SuppressPropertyChangedWarnings]
		private void OnActorChanged(Actor actor)
		{
			if (this.Appearance != null)
			{
				this.Appearance.PropertyChanged -= this.OnViewModelPropertyChanged;
				this.Appearance.Dispose();
			}

			this.skinColorMem?.Dispose();
			this.skinGlowMem?.Dispose();
			this.leftEyeColorMem?.Dispose();
			this.rightEyeColorMem?.Dispose();
			this.limbalRingColorMem?.Dispose();
			this.hairTintColorMem?.Dispose();
			this.hairGlowColorMem?.Dispose();
			this.highlightTintColorMem?.Dispose();

			if (this.lipTintMem != null)
			{
				this.lipTintMem?.Dispose();
				this.lipTintMem.ValueChanged -= this.OnLipTintMemChanged;
			}

			if (this.lipGlossMem != null)
			{
				this.lipGlossMem?.Dispose();
				this.lipGlossMem.ValueChanged -= this.OnLipTintMemChanged;
			}

			Application.Current.Dispatcher.Invoke(() => this.IsEnabled = false);
			this.Appearance = null;

			this.Hair = null;

			if (actor == null || (actor.Type != ActorTypes.Player && actor.Type != ActorTypes.EventNpc))
				return;

			this.skinColorMem = actor.GetMemory(Offsets.Main.SkinColor);
			this.skinColorMem.Bind(this, nameof(AppearanceEditor.SkinTint));
			this.skinColorMem.Name = "Skin Color";

			this.skinGlowMem = actor.GetMemory(Offsets.Main.SkinGloss);
			this.skinGlowMem.Bind(this, nameof(AppearanceEditor.SkinGlow));
			this.skinGlowMem.Name = "Skin Glow";

			this.leftEyeColorMem = actor.GetMemory(Offsets.Main.LeftEyeColor);
			this.leftEyeColorMem.Bind(this, nameof(AppearanceEditor.LeftEyeColor));
			this.leftEyeColorMem.Name = "Left Eye Color";

			this.rightEyeColorMem = actor.GetMemory(Offsets.Main.RightEyeColor);
			this.rightEyeColorMem.Bind(this, nameof(AppearanceEditor.RightEyeColor));
			this.rightEyeColorMem.Name = "Right Eye Color";

			this.limbalRingColorMem = actor.GetMemory(Offsets.Main.LimbalColor);
			this.limbalRingColorMem.Bind(this, nameof(AppearanceEditor.LimbalRingColor));
			this.limbalRingColorMem.Name = "Limbal Ring Color";

			this.hairTintColorMem = actor.GetMemory(Offsets.Main.HairColor);
			this.hairTintColorMem.Bind(this, nameof(AppearanceEditor.HairTint));
			this.hairTintColorMem.Name = "Hair Color";

			this.hairGlowColorMem = actor.GetMemory(Offsets.Main.HairGloss);
			this.hairGlowColorMem.Bind(this, nameof(AppearanceEditor.HairGlow));
			this.hairGlowColorMem.Name = "Gair Glow";

			this.highlightTintColorMem = actor.GetMemory(Offsets.Main.HairHiglight);
			this.highlightTintColorMem.Bind(this, nameof(AppearanceEditor.HighlightTint));
			this.highlightTintColorMem.Name = "Hair Highlight Color";
				
			this.lipTintMem = actor.GetMemory(Offsets.Main.MouthColor);
			this.lipTintMem.ValueChanged += this.OnLipTintMemChanged;
			this.lipTintMem.Name = "Lips Color";

			this.lipGlossMem = actor.GetMemory(Offsets.Main.MouthGloss);
			this.lipGlossMem.ValueChanged += this.OnLipTintMemChanged;
			this.lipGlossMem.Name = "Lips Gloss";
			this.OnLipTintMemChanged(null, null);

			this.Appearance = new AppearanceViewModel(actor);
			this.Appearance.PropertyChanged += this.OnViewModelPropertyChanged;
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = true;

				if (this.Appearance.Race == 0)
					this.Appearance.Race = ConceptMatrix.Appearance.Races.Hyur;

				this.Race = this.gameDataService.Races.Get((byte)this.Appearance.Race);
				this.TribeComboBox.ItemsSource = this.Race.Tribes;
				this.Tribe = this.gameDataService.Tribes.Get((byte)this.Appearance.Tribe);

				this.OnViewModelPropertyChanged(null, null);
			});
		}

		[SuppressPropertyChangedWarnings]
		private void OnLipTintMemChanged(object sender, object value)
		{
			this.LipTint = new Color4(this.lipTintMem.Value, this.lipGlossMem.Value);
		}

		private void AppearanceEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AppearanceEditor.LipTint))
			{
				if (this.LipTint != null)
				{
					Color4 val = (Color4)this.LipTint;
					this.lipTintMem.Value = val.Color;
					this.lipGlossMem.Value = val.A;
				}
			}
		}

		[SuppressPropertyChangedWarnings]
		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e == null || e.PropertyName == nameof(AppearanceViewModel.Race) || e.PropertyName == nameof(AppearanceViewModel.Tribe) || e.PropertyName == nameof(AppearanceViewModel.Gender))
			{
				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AppearanceEditor.Race)));
				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AppearanceEditor.Tribe)));

				this.HasFur = this.Appearance.Race == ConceptMatrix.Appearance.Races.Hrothgar;
				this.HasTail = this.Appearance.Race == ConceptMatrix.Appearance.Races.Hrothgar || this.Appearance.Race == ConceptMatrix.Appearance.Races.Miqote || this.Appearance.Race == ConceptMatrix.Appearance.Races.AuRa;
				this.HasLimbal = this.Appearance.Race == ConceptMatrix.Appearance.Races.AuRa;
				this.HasEars = this.Appearance.Race == ConceptMatrix.Appearance.Races.Viera;
				this.HasMuscles = !this.HasEars && !this.HasTail;
				this.HasGender = this.Appearance.Race != ConceptMatrix.Appearance.Races.Hrothgar && this.Appearance.Race != ConceptMatrix.Appearance.Races.Viera;

				bool canAge = this.Appearance.Tribe == ConceptMatrix.Appearance.Tribes.Midlander;
				canAge |= this.Appearance.Race == ConceptMatrix.Appearance.Races.Miqote && this.Appearance.Gender == ConceptMatrix.Appearance.Genders.Feminine;
				canAge |= this.Appearance.Race == ConceptMatrix.Appearance.Races.Elezen;
				canAge |= this.Appearance.Race == ConceptMatrix.Appearance.Races.AuRa && this.Appearance.Gender == ConceptMatrix.Appearance.Genders.Feminine;
				this.CanAge = canAge;

				if (this.Appearance.Tribe > 0)
				{
					this.Hair = this.gameDataService.CharacterMakeCustomize.GetHair(this.Appearance.Tribe, this.Appearance.Gender, this.Appearance.Hair);
				}
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.Hair))
			{
				this.Hair = this.gameDataService.CharacterMakeCustomize.GetHair(this.Appearance.Tribe, this.Appearance.Gender, this.Appearance.Hair);
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.Skintone))
			{
				this.SkinTint = null;
				this.SkinGlow = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.LimbalEyes))
			{
				this.LimbalRingColor = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.HairTone))
			{
				this.HairTint = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.Highlights))
			{
				this.HighlightTint = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.LipsToneFurPattern))
			{
				this.LipTint = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.LEyeColor))
			{
				this.LeftEyeColor = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.REyeColor))
			{
				this.RightEyeColor = null;
			}
		}

		private async void OnHairClicked(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();
			HairSelectorDrawer selector = new HairSelectorDrawer(this.Appearance.Gender, this.Appearance.Tribe, this.Appearance.Hair);

			selector.SelectionChanged += (v) =>
			{
				this.Appearance.Hair = v;
			};

			await viewService.ShowDrawer(selector, "Hair");
		}
	}
}
