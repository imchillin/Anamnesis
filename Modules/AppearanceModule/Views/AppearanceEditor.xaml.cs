// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	using AnAppearance = Anamnesis.Appearance;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	public partial class AppearanceEditor : UserControl, INotifyPropertyChanged
	{
		private readonly IGameDataService gameDataService;
		private readonly ISelectionService selectionService;

		public AppearanceEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.gameDataService = Services.Get<IGameDataService>();
			this.selectionService = Services.Get<ISelectionService>();

			this.GenderComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Genders));
			this.RaceComboBox.ItemsSource = this.gameDataService.Races.All;
			this.AgeComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Ages));
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

				if (value.Race == AnAppearance.Races.Hrothgar)
					this.Appearance.Gender = AnAppearance.Genders.Masculine;

				if (value.Race == AnAppearance.Races.Viera)
					this.Appearance.Gender = AnAppearance.Genders.Feminine;

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

		public AppearanceViewModel Appearance
		{
			get;
			private set;
		}

		public ExtendedAppearanceViewModel ExAppearance
		{
			get;
			private set;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.selectionService.ModeChanged += this.SelectionModeChanged;
			this.OnActorChanged(this.DataContext as Actor);
			this.SelectionModeChanged(this.selectionService.GetMode());
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.selectionService.ModeChanged -= this.SelectionModeChanged;
		}

		[SuppressPropertyChangedWarnings]
		private void OnActorChanged(Actor actor)
		{
			if (this.Appearance != null)
			{
				this.Appearance.PropertyChanged -= this.OnViewModelPropertyChanged;
				this.Appearance.Dispose();
			}

			Application.Current.Dispatcher.Invoke(() => this.IsEnabled = false);
			this.Appearance = null;

			this.Hair = null;

			if (actor == null || (actor.Type != ActorTypes.Player && actor.Type != ActorTypes.EventNpc))
				return;

			this.Appearance = new AppearanceViewModel(actor);
			this.Appearance.PropertyChanged += this.OnViewModelPropertyChanged;

			this.ExAppearance = new ExtendedAppearanceViewModel(actor);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = true;

				if (this.Appearance.Race == 0)
					this.Appearance.Race = AnAppearance.Races.Hyur;

				this.Race = this.gameDataService.Races.Get((byte)this.Appearance.Race);
				this.TribeComboBox.ItemsSource = this.Race.Tribes;
				this.Tribe = this.gameDataService.Tribes.Get((byte)this.Appearance.Tribe);

				this.OnViewModelPropertyChanged(null, null);
			});
		}

		[SuppressPropertyChangedWarnings]
		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e == null || e.PropertyName == nameof(AppearanceViewModel.Race) || e.PropertyName == nameof(AppearanceViewModel.Tribe) || e.PropertyName == nameof(AppearanceViewModel.Gender))
			{
				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AppearanceEditor.Race)));
				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AppearanceEditor.Tribe)));

				this.HasFur = this.Appearance.Race == AnAppearance.Races.Hrothgar;
				this.HasTail = this.Appearance.Race == AnAppearance.Races.Hrothgar || this.Appearance.Race == AnAppearance.Races.Miqote || this.Appearance.Race == AnAppearance.Races.AuRa;
				this.HasLimbal = this.Appearance.Race == AnAppearance.Races.AuRa;
				this.HasEars = this.Appearance.Race == AnAppearance.Races.Viera;
				this.HasMuscles = !this.HasEars && !this.HasTail;
				this.HasGender = this.Appearance.Race != AnAppearance.Races.Hrothgar && this.Appearance.Race != AnAppearance.Races.Viera;

				bool canAge = this.Appearance.Tribe == AnAppearance.Tribes.Midlander;
				canAge |= this.Appearance.Race == AnAppearance.Races.Miqote && this.Appearance.Gender == AnAppearance.Genders.Feminine;
				canAge |= this.Appearance.Race == AnAppearance.Races.Elezen;
				canAge |= this.Appearance.Race == AnAppearance.Races.AuRa && this.Appearance.Gender == AnAppearance.Genders.Feminine;
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
				this.ExAppearance.SkinTint = null;
				this.ExAppearance.SkinGlow = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.LimbalEyes))
			{
				this.ExAppearance.LimbalRingColor = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.HairTone))
			{
				this.ExAppearance.HairTint = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.Highlights))
			{
				this.ExAppearance.HighlightTint = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.LipsToneFurPattern))
			{
				this.ExAppearance.LipTint = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.LEyeColor))
			{
				this.ExAppearance.LeftEyeColor = null;
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.REyeColor))
			{
				this.ExAppearance.RightEyeColor = null;
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

		private async void SelectionModeChanged(Modes mode)
		{
			await Task.Delay(1);
			Application.Current.Dispatcher.Invoke(() =>
			{
				////this.ExtendedAppearanceArea.IsEnabled = mode == Modes.Overworld;
				this.AppearanceArea.IsEnabled = mode == Modes.Overworld;
			});
		}
	}
}
