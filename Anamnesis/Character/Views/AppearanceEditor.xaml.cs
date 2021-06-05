// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	using AnAppearance = Anamnesis.Memory.Appearance;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AppearanceEditor : UserControl
	{
		private bool appearanceLocked = false;

		public AppearanceEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.GenderComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Genders));
			this.AgeComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Ages));

			List<IRace> races = new List<IRace>();
			foreach (IRace race in GameDataService.Races)
			{
				if (race.Key == 0)
					continue;

				races.Add(race);
			}

			this.RaceComboBox.ItemsSource = races;
		}

		public bool HasGender { get; set; }
		public bool HasFur { get; set; }
		public bool HasLimbal { get; set; }
		public bool HasTail { get; set; }
		public bool HasEars { get; set; }
		public bool HasMuscles { get; set; }
		public bool CanAge { get; set; }
		public ICharaMakeCustomize? Hair { get; set; }
		public ICharaMakeCustomize? FacePaint { get; set; }
		public IRace? Race { get; set; }
		public ITribe? Tribe { get; set; }

		public double HeightCm { get; set; }
		public string? HeightFeet { get; set; }

		public AppearanceViewModel? Appearance
		{
			get;
			private set;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorViewModel);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as ActorViewModel);
		}

		private void OnActorChanged(ActorViewModel? actor)
		{
			Application.Current.Dispatcher.Invoke(() => this.IsEnabled = false);

			this.Hair = null;
			this.FacePaint = null;

			if (this.Appearance != null)
				this.Appearance.PropertyChanged -= this.OnAppearancePropertyChanged;

			if (actor == null || actor.Customize == null)
				return;

			this.Appearance = actor.Customize;
			this.Appearance.PropertyChanged += this.OnAppearancePropertyChanged;

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.UpdateRaceAndTribe();
			});
		}

		private void OnAppearancePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (this.appearanceLocked)
				return;

			if (e.PropertyName == nameof(AppearanceViewModel.Race) ||
				e.PropertyName == nameof(AppearanceViewModel.Tribe) ||
				e.PropertyName == nameof(AppearanceViewModel.Hair) ||
				e.PropertyName == nameof(AppearanceViewModel.FacePaint))
			{
				Application.Current.Dispatcher.Invoke(this.UpdateRaceAndTribe);
			}
		}

		private void UpdateRaceAndTribe()
		{
			if (GameDataService.Races == null)
				throw new Exception("Races not loaded");

			if (GameDataService.Tribes == null)
				throw new Exception("Tribes not loaded");

			if (GameDataService.CharacterMakeCustomize == null)
				throw new Exception("CharacterMakeCustomize not loaded");

			if (this.Appearance == null)
			{
				this.IsEnabled = false;
				return;
			}

			if (this.Appearance.Race == 0 || this.Appearance.Race > AnAppearance.Races.Viera)
			{
				this.IsEnabled = false;
				return;
			}

			this.Race = GameDataService.Races.Get((uint)this.Appearance.Race);

			// Something has gone terribly wrong.
			if (this.Race == null)
			{
				this.IsEnabled = false;
				return;
			}

			this.IsEnabled = true;

			this.RaceComboBox.SelectedItem = this.Race;

			this.TribeComboBox.ItemsSource = this.Race.Tribes;

			this.Tribe = GameDataService.Tribes.Get((uint)this.Appearance.Tribe);

			if (this.Appearance.Tribe == 0 || this.Tribe == null)
				this.Appearance.Tribe = this.Race.Tribes.First().Tribe;

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
			canAge |= this.Appearance.Race == AnAppearance.Races.AuRa;
			this.CanAge = canAge;

			if (this.Appearance.Tribe > 0)
			{
				this.Hair = GameDataService.CharacterMakeCustomize.GetFeature(Features.Hair, this.Appearance.Tribe, this.Appearance.Gender, this.Appearance.Hair);
				this.FacePaint = GameDataService.CharacterMakeCustomize.GetFeature(Features.FacePaint, this.Appearance.Tribe, this.Appearance.Gender, this.Appearance.FacePaint);
			}

			this.IsEnabled = true;
		}

		private void OnGenderChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.Appearance == null)
				return;

			AnAppearance.Genders? gender = this.GenderComboBox.SelectedItem as AnAppearance.Genders?;

			if (gender == null)
				return;

			// Do not change to masculine gender when a young miqo or aura as it will crash the game
			if (this.Appearance.Age == AnAppearance.Ages.Young && (this.Appearance.Race == AnAppearance.Races.Miqote))
			{
				this.Appearance.Age = AnAppearance.Ages.Normal;
			}

			this.Appearance.Gender = (AnAppearance.Genders)gender;

			this.UpdateRaceAndTribe();
		}

		private async void OnHairClicked(object sender, RoutedEventArgs e)
		{
			if (this.Appearance == null)
				return;

			CustomizeFeatureSelectorDrawer selector = new CustomizeFeatureSelectorDrawer(Features.Hair, this.Appearance.Gender, this.Appearance.Tribe, this.Appearance.Hair);
			selector.SelectionChanged += (v) =>
			{
				this.Appearance.Hair = v;
			};

			await ViewService.ShowDrawer(selector);
		}

		private async void OnFacePaintClicked(object sender, RoutedEventArgs e)
		{
			if (this.Appearance == null)
				return;

			CustomizeFeatureSelectorDrawer selector = new CustomizeFeatureSelectorDrawer(Features.FacePaint, this.Appearance.Gender, this.Appearance.Tribe, this.Appearance.FacePaint);
			selector.SelectionChanged += (v) =>
			{
				this.Appearance.FacePaint = v;
			};

			await ViewService.ShowDrawer(selector);
		}

		private void OnRaceChanged(object sender, SelectionChangedEventArgs e)
		{
			IRace? race = this.RaceComboBox.SelectedItem as IRace;

			if (race == null || this.Appearance == null)
				return;

			// did we change?
			if (race == this.Race)
				return;

			if (race.Race == AnAppearance.Races.Hrothgar)
				this.Appearance.Gender = AnAppearance.Genders.Masculine;

			if (race.Race == AnAppearance.Races.Viera)
				this.Appearance.Gender = AnAppearance.Genders.Feminine;

			// reset age when chaing race
			this.Appearance.Age = AnAppearance.Ages.Normal;

			if (this.Race == race)
				return;

			this.appearanceLocked = true;

			int oldTribeIndex = this.GetTribeIndex(this.Race, this.Tribe);

			this.Race = race;

			this.TribeComboBox.ItemsSource = this.Race.Tribes;

			if (oldTribeIndex < 0 || oldTribeIndex > this.Race.Tribes.Length)
			{
				this.Tribe = this.Race.Tribes.First();
			}
			else
			{
				this.Tribe = this.Race.Tribes[oldTribeIndex];
			}

			this.TribeComboBox.SelectedItem = this.Tribe;

			this.Appearance.Race = this.Race.Race;
			this.Appearance.Tribe = this.Tribe.Tribe;

			this.UpdateRaceAndTribe();

			this.appearanceLocked = false;
		}

		private void OnTribeChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.Appearance == null)
				return;

			ITribe? tribe = this.TribeComboBox.SelectedItem as ITribe;

			if (tribe == null || this.Tribe == tribe)
				return;

			// reset age when chaing tribe
			this.Appearance.Age = AnAppearance.Ages.Normal;

			this.Tribe = tribe;
			this.Appearance.Tribe = this.Tribe.Tribe;

			this.UpdateRaceAndTribe();
		}

		private int GetTribeIndex(IRace? race, ITribe? tribe)
		{
			if (race == null || tribe == null)
				return -1;

			ITribe[] tribes = race.Tribes;
			for (int i = 0; i < tribes.Length; i++)
			{
				if (tribes[i] == tribe)
				{
					return i;
				}
			}

			return -1;
		}

		/*private void CalculateHeight()
		{
			bool isFeminine = this.Appearance.Gender == AnAppearance.Genders.Feminine;
			double min;
			double max;

			min = this.Tribe.Tribe switch
			{
				AnAppearance.Tribes.Midlander => isFeminine ? 157.4 : 168.0,
				AnAppearance.Tribes.Highlander => isFeminine ? 173.4 : 184.8,
				AnAppearance.Tribes.Wildwood => isFeminine ? 183.5 : 194.1,
				AnAppearance.Tribes.Duskwight => isFeminine ? 183.5 : 194.1,
				AnAppearance.Tribes.Plainsfolk => isFeminine ? 86.9 : 86.9,
				AnAppearance.Tribes.Dunesfolk => isFeminine ? 86.9 : 86.9,
				AnAppearance.Tribes.SeekerOfTheSun => isFeminine ? 149.7 : 159.2,
				AnAppearance.Tribes.KeeperOfTheMoon => isFeminine ? 149.7 : 159.2,
				AnAppearance.Tribes.SeaWolf => isFeminine ? 192.0 : 213.5,
				AnAppearance.Tribes.Hellsguard => isFeminine ? 192.0 : 213.5,
				AnAppearance.Tribes.Raen => isFeminine ? 146.0 : 203.0,
				AnAppearance.Tribes.Xaela => isFeminine ? 146.0 : 203.0,
				AnAppearance.Tribes.Helions => 196.2,
				AnAppearance.Tribes.TheLost => 196.2,
				AnAppearance.Tribes.Rava => 178.8,
				AnAppearance.Tribes.Veena => 178.8,

				_ => throw new NotSupportedException(),
			};

			max = this.Tribe.Tribe switch
			{
				AnAppearance.Tribes.Midlander => isFeminine ? 170.0 : 182.0,
				AnAppearance.Tribes.Highlander => isFeminine ? 187.6 : 200.2,
				AnAppearance.Tribes.Wildwood => isFeminine ? 198.4 : 209.8,
				AnAppearance.Tribes.Duskwight => isFeminine ? 198.4 : 209.8,
				AnAppearance.Tribes.Plainsfolk => isFeminine ? 97.0 : 97.0,
				AnAppearance.Tribes.Dunesfolk => isFeminine ? 97.0 : 97.0,
				AnAppearance.Tribes.SeekerOfTheSun => isFeminine ? 162.2 : 173.2,
				AnAppearance.Tribes.KeeperOfTheMoon => isFeminine ? 162.2 : 173.2,
				AnAppearance.Tribes.SeaWolf => isFeminine ? 222.7 : 230.4,
				AnAppearance.Tribes.Hellsguard => isFeminine ? 222.7 : 230.4,
				AnAppearance.Tribes.Raen => isFeminine ? 158.5 : 217.0,
				AnAppearance.Tribes.Xaela => isFeminine ? 158.5 : 217.0,
				AnAppearance.Tribes.Helions => 212.9,
				AnAppearance.Tribes.TheLost => 217.0,
				AnAppearance.Tribes.Rava => 191.4,
				AnAppearance.Tribes.Veena => 191.4,

				_ => throw new NotSupportedException(),
			};

			double h = this.Appearance.Height / 100.0;
			h = (min * (1 - h)) + (max * h);

			this.HeightCm = Math.Round(h);

			double feet = (this.HeightCm / 2.54) / 12.0;
			int iFeet = (int)feet;
			int inches = (int)((feet - (double)iFeet) * 12.0);
			this.HeightFeet = iFeet + "' " + inches + "''";
		}*/
	}
}
