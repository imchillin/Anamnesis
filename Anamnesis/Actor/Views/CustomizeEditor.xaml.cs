// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static Anamnesis.Memory.ActorCustomizeMemory;

/// <summary>
/// Interaction logic for AppearancePage.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class CustomizeEditor : UserControl
{
	/// <summary>
	/// Tribes for which the face options need to be offset by -100
	/// to match the internal face ids used by the game.
	/// </summary>
	/// <remarks>
	/// Au Ra, Hrothgar and Viera use face ids 1 - 100 regardless of the tribe.
	/// </remarks>
	private static readonly HashSet<Tribes> s_offsetTargets = new()
	{
		Tribes.Highlander,
		Tribes.Duskwight,
		Tribes.Dunesfolk,
		Tribes.KeeperOfTheMoon,
		Tribes.Hellsguard,
	};

	private readonly DispatcherTimer debounceTimer;

	public CustomizeEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.GenderComboBox.ItemsSource = Enum.GetValues<Genders>();
		this.AgeComboBox.ItemsSource =
			Enum.GetValues<Ages>()
				.Cast<ActorCustomizeMemory.Ages>()
				.Where(age => age != ActorCustomizeMemory.Ages.None);

		List<Race> races = GameDataService.Races.Where(race => race.RowId != 0).ToList();
		this.RaceComboBox.ItemsSource = races;

		this.debounceTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(100),
		};
		this.debounceTimer.Tick += this.DebounceTimer_Tick;
	}

	public List<byte> ValidFaceOptions { get; set; } = new();
	public List<byte> ValidTailEarsOptions { get; set; } = new();
	public Ages SelectedAge { get; set; } = Ages.Normal;

	public bool HasGender { get; set; }
	public bool HasFur { get; set; }
	public bool HasTail { get; set; }
	public bool HasEars { get; set; }
	public bool HasEarsTail { get; set; }
	[DependsOn(nameof(Customize), nameof(HasEars), nameof(HasTail))]
	public bool HasSeparateTailEarsModel => this.Customize?.Race != Races.Elezen && this.Customize?.Race != Races.Lalafel;
	public bool HasMuscles { get; set; }
	public bool CanAge { get; set; }
	public CharaMakeCustomize? Hair { get; set; }
	public CharaMakeCustomize? FacePaint { get; set; }
	public Race? Race { get; set; }
	public Tribe? Tribe { get; set; }
	public ActorMemory? Actor { get; private set; }
	public ActorCustomizeMemory? Customize { get; private set; }

	private static int GetTribeIndex(Race? race, Tribe? tribe)
	{
		if (race == null || tribe == null)
			return -1;

		Tribe[] tribes = race.Value.Tribes;
		for (int i = 0; i < tribes.Length; i++)
		{
			if (tribes[i].RowId == tribe.Value.RowId)
				return i;
		}

		return -1;
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

		Application.Current.Dispatcher.Invoke(this.UpdateRaceAndTribe);
	}

	private void DebounceTimer_Tick(object? sender, EventArgs e)
	{
		this.debounceTimer.Stop();

		// Based on one or multiple property updates, reflect the changes
		// in the rest of the customize editor components.
		Application.Current.Dispatcher.Invoke(this.UpdateRaceAndTribe);
	}

	private void OnAppearancePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ActorCustomizeMemory.Gender) ||
			e.PropertyName == nameof(ActorCustomizeMemory.Race) ||
			e.PropertyName == nameof(ActorCustomizeMemory.Tribe) ||
			e.PropertyName == nameof(ActorCustomizeMemory.Hair) ||
			e.PropertyName == nameof(ActorCustomizeMemory.FacePaint))
		{
			if (Application.Current == null)
				return;

			// Restart the debounce timer if it's already running, otherwise start it.
			// Note: A debounce timer is used here to ensure that the update is called
			// only once after all properties have been updated.
			this.debounceTimer.Stop();
			this.debounceTimer.Start();
		}
	}

	private void UpdateFaceOptions(Tribes tribe, Genders gender, Ages age)
	{
		if (this.Customize == null)
			return;

		var dataPath = DataPathResolver.ToDataPath(tribe, gender, age != Ages.Normal);
		if (dataPath.HasValue && CustomizeOptionsCache.ValidFaceIds.TryGetValue(dataPath.Value, out var validFaces))
		{
			// Apply offset logic for specific tribes
			if (s_offsetTargets.Contains(tribe))
			{
				this.ValidFaceOptions = validFaces
					.Where(v => v > 100)
					.Select(v => v <= 200 ? (byte)(v - 100) : v)
					.ToList();
			}
			else
			{
				this.ValidFaceOptions = validFaces.Where(v => v <= 100 || v > 200).ToList();
			}
		}
		else
		{
			this.ValidFaceOptions = [];
		}

		if (this.ValidFaceOptions.Count > 0)
		{
			if (this.ValidFaceOptions.Contains(this.Customize.Head))
				this.FaceComboBox.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateTarget();
			else
				this.Customize.Head = this.ValidFaceOptions[0];
		}

		this.OnPropertyChanged(nameof(this.ValidFaceOptions));
	}

	private void UpdateTailEarsOptions(Tribes tribe, Genders gender, Ages age)
	{
		if (this.Customize == null)
			return;

		var dataPath = DataPathResolver.ToDataPath(tribe, gender, age != Ages.Normal);
		if (dataPath.HasValue && CustomizeOptionsCache.ValidTailEarIds.TryGetValue(dataPath.Value, out var validTailEars))
			this.ValidTailEarsOptions = validTailEars;
		else
			this.ValidTailEarsOptions = new List<byte>();

		if (this.ValidTailEarsOptions.Count > 0)
		{
			if (this.ValidTailEarsOptions.Contains(this.Customize.TailEarsType))
				this.TailEarsCombobox.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateTarget();
			else
				this.Customize.TailEarsType = this.ValidTailEarsOptions[0];
		}

		this.OnPropertyChanged(nameof(this.ValidTailEarsOptions));
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

		if (this.Customize.Race == 0 || this.Customize.Race > Races.Viera)
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

		// Unsubscribe from events to avoid the callbacks from being called
		this.RaceComboBox.SelectionChanged -= this.OnRaceChanged;
		this.TribeComboBox.SelectionChanged -= this.OnTribeChanged;
		this.AgeComboBox.SelectionChanged -= this.OnAgeChanged;

		this.RaceComboBox.SelectedItem = this.Race;
		this.TribeComboBox.ItemsSource = this.Race.Value.Tribes;

		if (!Enum.IsDefined<Tribes>((Tribes)this.Customize.Tribe))
			this.Customize.Tribe = Tribes.Midlander;

		this.Tribe = GameDataService.Tribes.GetRow((uint)this.Customize.Tribe);

		if (this.Customize.Tribe == 0 || this.Tribe == null)
			this.Customize.Tribe = this.Race.Value.Tribes.First().CustomizeTribe;

		this.TribeComboBox.SelectedItem = this.Tribe;

		this.UpdateFaceOptions(this.Customize.Tribe, this.Customize.Gender, this.Customize.Age);
		this.UpdateTailEarsOptions(this.Customize.Tribe, this.Customize.Gender, this.Customize.Age);

		// Re-subscribe to selection changed events
		this.AgeComboBox.SelectionChanged += this.OnAgeChanged;
		this.TribeComboBox.SelectionChanged += this.OnTribeChanged;
		this.RaceComboBox.SelectionChanged += this.OnRaceChanged;

		this.HasTail = this.Customize.Race is Races.Hrothgar or Races.Miqote or Races.AuRa;
		this.HasEars = this.Customize.Race is Races.Viera or Races.Lalafel or Races.Elezen;
		this.HasFur = this.Customize.Race == Races.Hrothgar;
		this.HasEarsTail = this.HasTail || this.HasEars;
		this.HasMuscles = !this.HasEars && !this.HasTail;
		this.HasGender = true;

		this.OnPropertyChanged(nameof(this.HasSeparateTailEarsModel));

		bool canAge = this.Customize.Tribe == Tribes.Midlander;
		canAge |= this.Customize.Race == Races.Miqote && this.Customize.Gender == Genders.Feminine;
		canAge |= this.Customize.Race == Races.Elezen;
		canAge |= this.Customize.Race == Races.AuRa;
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
		Genders? gender = this.GenderComboBox.SelectedItem as Genders?;
		if (gender == null || this.Customize == null)
			return;

		// Do not change to masculine gender when a young miqo or aura as it will crash the game
		if (this.Customize.Age == Ages.Young && (this.Customize.Race == Races.Miqote))
		{
			this.Customize.Age = Ages.Normal;
		}

		this.Customize.Gender = (Genders)gender;
	}

	private void OnAgeChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.Customize == null)
			return;

		if (this.AgeComboBox.SelectedItem is not Ages newAge)
			return;

		this.UpdateFaceOptions(this.Customize.Tribe, this.Customize.Gender, newAge);
		this.UpdateTailEarsOptions(this.Customize.Tribe, this.Customize.Gender, newAge);
		this.Customize.Age = newAge;
	}

	private async void OnHairClicked(object sender, RoutedEventArgs e)
	{
		if (this.Customize == null)
			return;

		CustomizeFeatureSelectorDrawer selector = new(CustomizeSheet.Features.Hair, this.Customize.Gender, this.Customize.Tribe, this.Customize.Hair);
		selector.SelectionChanged += (v) => { this.Customize.Hair = v; };

		await ViewService.ShowDrawer(selector);
	}

	private async void OnFacePaintClicked(object sender, RoutedEventArgs e)
	{
		if (this.Customize == null)
			return;

		CustomizeFeatureSelectorDrawer selector = new(CustomizeSheet.Features.FacePaint, this.Customize.Gender, this.Customize.Tribe, this.Customize.FacePaint);
		selector.SelectionChanged += (v) => { this.Customize.FacePaint = v; };

		await ViewService.ShowDrawer(selector);
	}

	private void OnRaceChanged(object sender, SelectionChangedEventArgs e)
	{
		Race? race = this.RaceComboBox.SelectedItem as Race?;
		if (race == null || this.Customize == null || this.Race == null || this.Race.Value.RowId == race.Value.RowId)
			return;

		// Unsubscribe to avoid the callback from being called
		this.TribeComboBox.SelectionChanged -= this.OnTribeChanged;

		// Reset age when changing race
		this.AgeComboBox.SelectedValue = Ages.Normal;

		int oldTribeIndex = GetTribeIndex(this.Race, this.Tribe);

		this.Race = race;

		this.TribeComboBox.ItemsSource = this.Race.Value.Tribes;

		if (oldTribeIndex < 0 || oldTribeIndex > this.Race.Value.Tribes.Length)
		{
			this.Tribe = this.Race.Value.Tribes.First();
		}
		else
		{
			this.Tribe = this.Race.Value.Tribes[oldTribeIndex];
		}

		this.TribeComboBox.SelectedItem = this.Tribe;

		// Re-subscribe to tribe changed event
		this.TribeComboBox.SelectionChanged += this.OnTribeChanged;

		this.Customize.Race = this.Race.Value.CustomizeRace;
		this.Customize.Tribe = this.Tribe.Value.CustomizeTribe;
	}

	private void OnTribeChanged(object sender, SelectionChangedEventArgs e)
	{
		Tribe? tribe = this.TribeComboBox.SelectedItem as Tribe?;
		if (tribe == null || this.Customize == null || this.Tribe == null || this.Tribe.Value.RowId == tribe.Value.RowId)
			return;

		// Reset age when changing tribe
		this.AgeComboBox.SelectedValue = Ages.Normal;

		this.Tribe = tribe;
		this.Customize.Tribe = this.Tribe.Value.CustomizeTribe;
	}
}
