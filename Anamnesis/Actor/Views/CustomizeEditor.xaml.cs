// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;

using AnAppearance = Anamnesis.Memory.ActorCustomizeMemory;

/// <summary>
/// Interaction logic for AppearancePage.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class CustomizeEditor : UserControl
{
	private bool appearanceLocked = false;

	public CustomizeEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.GenderComboBox.ItemsSource = Enum.GetValues(typeof(ActorCustomizeMemory.Genders));
		this.AgeComboBox.ItemsSource = Enum.GetValues(typeof(ActorCustomizeMemory.Ages));

		List<Race> races = new List<Race>();
		foreach (Race race in GameDataService.Races)
		{
			if (race.RowId == 0)
				continue;

			races.Add(race);
		}

		this.RaceComboBox.ItemsSource = races;
	}

	public bool HasGender { get; set; }
	public bool HasFur { get; set; }
	public bool HasTail { get; set; }
	public bool HasEars { get; set; }
	public bool HasEarsTail { get; set; }
	public bool HasMuscles { get; set; }
	public bool CanAge { get; set; }
	public CharaMakeCustomize? Hair { get; set; }
	public CharaMakeCustomize? FacePaint { get; set; }
	public Race? Race { get; set; }
	public Tribe? Tribe { get; set; }

	public double HeightCm { get; set; }
	public string? HeightFeet { get; set; }

	public ActorMemory? Actor
	{
		get;
		private set;
	}

	public ActorCustomizeMemory? Customize
	{
		get;
		private set;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ActorMemory);
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.OnActorChanged(this.DataContext as ActorMemory);
	}

	private void OnActorChanged(ActorMemory? actor)
	{
		this.Actor = actor;
		Application.Current.Dispatcher.Invoke(() => this.IsEnabled = false);

		this.Hair = null;
		this.FacePaint = null;

		if (this.Customize != null)
			this.Customize.PropertyChanged -= this.OnAppearancePropertyChanged;

		if (actor == null || actor.Customize == null)
			return;

		this.Customize = actor.Customize;
		this.Customize.PropertyChanged += this.OnAppearancePropertyChanged;

		Application.Current.Dispatcher.Invoke(() =>
		{
			this.UpdateRaceAndTribe();
		});
	}

	private void OnAppearancePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (this.appearanceLocked)
			return;

		if (e.PropertyName == nameof(ActorCustomizeMemory.Race) ||
			e.PropertyName == nameof(ActorCustomizeMemory.Tribe) ||
			e.PropertyName == nameof(ActorCustomizeMemory.Hair) ||
			e.PropertyName == nameof(ActorCustomizeMemory.FacePaint))
		{
			if (Application.Current == null)
				return;

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

		if (this.Customize == null)
		{
			this.IsEnabled = false;
			return;
		}

		if (this.Customize.Race == 0 || this.Customize.Race > AnAppearance.Races.Viera)
		{
			this.IsEnabled = false;
			return;
		}

		this.Race = GameDataService.Races.GetRow((uint)this.Customize.Race);

		// Something has gone terribly wrong.
		if (this.Race == null)
		{
			this.IsEnabled = false;
			return;
		}

		this.IsEnabled = true;

		this.RaceComboBox.SelectedItem = this.Race;
		this.TribeComboBox.ItemsSource = this.Race.Tribes;

		if (!Enum.IsDefined<ActorCustomizeMemory.Tribes>((ActorCustomizeMemory.Tribes)this.Customize.Tribe))
			this.Customize.Tribe = ActorCustomizeMemory.Tribes.Midlander;

		this.Tribe = GameDataService.Tribes.Get((uint)this.Customize.Tribe);

		if (this.Customize.Tribe == 0 || this.Tribe == null)
			this.Customize.Tribe = this.Race.Tribes.First().CustomizeTribe;

		this.TribeComboBox.SelectedItem = this.Tribe;

		this.HasFur = this.Customize.Race == AnAppearance.Races.Hrothgar;
		this.HasTail = this.Customize.Race == AnAppearance.Races.Hrothgar || this.Customize.Race == AnAppearance.Races.Miqote || this.Customize.Race == AnAppearance.Races.AuRa;
		this.HasEars = this.Customize.Race == AnAppearance.Races.Viera || this.Customize.Race == AnAppearance.Races.Lalafel || this.Customize.Race == AnAppearance.Races.Elezen;
		this.HasEarsTail = this.HasTail | this.HasEars;
		this.HasMuscles = !this.HasEars && !this.HasTail;
		this.HasGender = this.Customize.Race != AnAppearance.Races.Hrothgar;

		bool canAge = this.Customize.Tribe == AnAppearance.Tribes.Midlander;
		canAge |= this.Customize.Race == AnAppearance.Races.Miqote && this.Customize.Gender == AnAppearance.Genders.Feminine;
		canAge |= this.Customize.Race == AnAppearance.Races.Elezen;
		canAge |= this.Customize.Race == AnAppearance.Races.AuRa;
		this.CanAge = canAge;

		if (this.Customize.Tribe > 0)
		{
			this.Hair = GameDataService.CharacterMakeCustomize.GetFeature(CustomizeSheet.Features.Hair, this.Customize.Tribe, this.Customize.Gender, this.Customize.Hair);
			this.FacePaint = GameDataService.CharacterMakeCustomize.GetFeature(CustomizeSheet.Features.FacePaint, this.Customize.Tribe, this.Customize.Gender, this.Customize.FacePaint);
		}

		this.IsEnabled = true;
	}

	private void OnGenderChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.Customize == null)
			return;

		AnAppearance.Genders? gender = this.GenderComboBox.SelectedItem as AnAppearance.Genders?;

		if (gender == null)
			return;

		// Do not change to masculine gender when a young miqo or aura as it will crash the game
		if (this.Customize.Age == AnAppearance.Ages.Young && (this.Customize.Race == AnAppearance.Races.Miqote))
		{
			this.Customize.Age = AnAppearance.Ages.Normal;
		}

		this.Customize.Gender = (AnAppearance.Genders)gender;

		this.UpdateRaceAndTribe();
	}

	private void OnHairClicked(object sender, RoutedEventArgs e)
	{
		if (this.Customize == null)
			return;

		CustomizeFeatureSelectorDrawer selector = new CustomizeFeatureSelectorDrawer(CustomizeSheet.Features.Hair, this.Customize.Gender, this.Customize.Tribe, this.Customize.Hair);
		selector.SelectionChanged += (v) =>
		{
			this.Customize.Hair = v;
		};

		throw new NotImplementedException();
	}

	private void OnFacePaintClicked(object sender, RoutedEventArgs e)
	{
		if (this.Customize == null)
			return;

		CustomizeFeatureSelectorDrawer selector = new CustomizeFeatureSelectorDrawer(CustomizeSheet.Features.FacePaint, this.Customize.Gender, this.Customize.Tribe, this.Customize.FacePaint);
		selector.SelectionChanged += (v) =>
		{
			this.Customize.FacePaint = v;
		};

		throw new NotImplementedException();
	}

	private void OnRaceChanged(object sender, SelectionChangedEventArgs e)
	{
		Race? race = this.RaceComboBox.SelectedItem as Race;

		if (race == null || this.Customize == null)
			return;

		// did we change?
		if (race == this.Race)
			return;

		if (race.CustomizeRace == AnAppearance.Races.Hrothgar)
			this.Customize.Gender = AnAppearance.Genders.Masculine;

		// reset age when chaing race
		this.Customize.Age = AnAppearance.Ages.Normal;

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

		this.Customize.Race = this.Race.CustomizeRace;
		this.Customize.Tribe = this.Tribe.CustomizeTribe;

		this.UpdateRaceAndTribe();

		this.appearanceLocked = false;
	}

	private void OnTribeChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.Customize == null)
			return;

		Tribe? tribe = this.TribeComboBox.SelectedItem as Tribe;

		if (tribe == null || this.Tribe == tribe)
			return;

		// reset age when chaing tribe
		this.Customize.Age = AnAppearance.Ages.Normal;

		this.Tribe = tribe;
		this.Customize.Tribe = this.Tribe.CustomizeTribe;

		this.UpdateRaceAndTribe();
	}

	private int GetTribeIndex(Race? race, Tribe? tribe)
	{
		if (race == null || tribe == null)
			return -1;

		Tribe[] tribes = race.Tribes;
		for (int i = 0; i < tribes.Length; i++)
		{
			if (tribes[i] == tribe)
			{
				return i;
			}
		}

		return -1;
	}
}
