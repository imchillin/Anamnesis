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
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class AppearanceEditor : UserControl
	{
		private readonly IGameDataService gameDataService;

		public AppearanceEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.gameDataService = Services.Get<IGameDataService>();

			this.GenderComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Genders));
			this.RaceComboBox.ItemsSource = this.gameDataService.Races.All;
			this.AgeComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Ages));
		}

		public bool HasGender { get; set; }
		public bool HasFur { get; set; }
		public bool HasLimbal { get; set; }
		public bool HasTail { get; set; }
		public bool HasEars { get; set; }
		public bool HasMuscles { get; set; }
		public bool CanAge { get; set; }
		public ICharaMakeCustomize Hair { get; set; }
		public IRace Race { get; set; }
		public ITribe Tribe { get; set; }

		public AppearanceViewModel Appearance
		{
			get;
			private set;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as Actor);
		}

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

			if (actor == null || !actor.IsCustomizable())
				return;

			this.Appearance = new AppearanceViewModel(actor);
			this.Appearance.PropertyChanged += this.OnViewModelPropertyChanged;

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = true;

				if (this.Appearance.Race == 0)
					this.Appearance.Race = AnAppearance.Races.Hyur;

				this.UpdateRaceAndTribe();
			});
		}

		private void UpdateRaceAndTribe()
		{
			if (this.Appearance.Race == 0)
				this.Appearance.Race = AnAppearance.Races.Hyur;

			this.Race = this.gameDataService.Races.Get((int)this.Appearance.Race);
			this.RaceComboBox.SelectedItem = this.Race;

			this.TribeComboBox.ItemsSource = this.Race.Tribes;

			if (this.Appearance.Tribe == 0)
				this.Appearance.Tribe = this.Race.Tribes.First().Tribe;

			this.Tribe = this.gameDataService.Tribes.Get((int)this.Appearance.Tribe);
			this.TribeComboBox.SelectedItem = this.Tribe;

			this.HasFur = this.Appearance.Race == AnAppearance.Races.Hrothgar;
			this.HasTail = this.Appearance.Race == AnAppearance.Races.Hrothgar || this.Appearance.Race == AnAppearance.Races.Miqote || this.Appearance.Race == AnAppearance.Races.AuRa;
			this.HasLimbal = this.Appearance.Race == AnAppearance.Races.AuRa;
			this.HasEars = this.Appearance.Race == AnAppearance.Races.Viera || this.Appearance.Race == AnAppearance.Races.Lalafel || this.Appearance.Race == AnAppearance.Races.Elezen;
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

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e == null || e.PropertyName == nameof(AppearanceViewModel.Race) || e.PropertyName == nameof(AppearanceViewModel.Tribe) || e.PropertyName == nameof(AppearanceViewModel.Gender))
			{
				this.UpdateRaceAndTribe();
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.Hair))
			{
				this.Hair = this.gameDataService.CharacterMakeCustomize.GetHair(this.Appearance.Tribe, this.Appearance.Gender, this.Appearance.Hair);
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

		private void OnRaceChanged(object sender, SelectionChangedEventArgs e)
		{
			IRace race = this.RaceComboBox.SelectedItem as IRace;

			if (race.Race == AnAppearance.Races.Hrothgar)
				this.Appearance.Gender = AnAppearance.Genders.Masculine;

			if (race.Race == AnAppearance.Races.Viera)
				this.Appearance.Gender = AnAppearance.Genders.Feminine;

			if (this.Race == race)
				return;

			this.Race = race;

			this.Appearance.Race = this.Race.Race;
			this.TribeComboBox.ItemsSource = this.Race.Tribes;
			this.Tribe = this.Race.Tribes.First();
			this.TribeComboBox.SelectedItem = this.Tribe;

			this.UpdateRaceAndTribe();
		}

		private void OnTribeChanged(object sender, SelectionChangedEventArgs e)
		{
			ITribe tribe = this.TribeComboBox.SelectedItem as ITribe;

			if (tribe == null || this.Tribe == tribe)
				return;

			this.Tribe = tribe;
			this.Appearance.Tribe = this.Tribe.Tribe;

			this.UpdateRaceAndTribe();
		}
	}
}
