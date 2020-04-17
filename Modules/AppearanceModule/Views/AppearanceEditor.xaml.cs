// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Files;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.GameData;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	public partial class AppearanceEditor : UserControl, INotifyPropertyChanged
	{
		private IGameDataService gameDataService;

		public AppearanceEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.gameDataService = Services.Get<IGameDataService>();
			ISelectionService selectionService = Services.Get<ISelectionService>();

			this.GenderComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Genders));
			this.RaceComboBox.ItemsSource = this.gameDataService.Races.All;
			this.AgeComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Ages));

			selectionService.SelectionChanged += this.OnSelectionChanged;
			this.OnSelectionChanged(selectionService.CurrentSelection);
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
				if (this.ViewModel == null)
					return null;

				return this.gameDataService.Races.Get((int)this.ViewModel.Race);
			}

			set
			{
				if (value == null)
					return;

				if (value.Race == Appearance.Races.Hrothgar)
					this.ViewModel.Gender = Appearance.Genders.Masculine;

				if (value.Race == Appearance.Races.Vierra)
					this.ViewModel.Gender = Appearance.Genders.Feminine;

				this.ViewModel.Race = value.Race;
				this.TribeComboBox.ItemsSource = value.Tribes;
				this.Tribe = value.Tribes.First();
			}
		}

		public ITribe Tribe
		{
			get
			{
				if (this.ViewModel == null)
					return null;

				return this.gameDataService.Tribes.Get((int)this.ViewModel.Tribe);
			}

			set
			{
				if (value == null)
					return;

				this.ViewModel.Tribe = value.Tribe;
			}
		}

		public AppearanceViewModel ViewModel
		{
			get;
			private set;
		}

		private void OnSelectionChanged(Selection selection)
		{
			if (this.ViewModel != null)
			{
				this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
				this.ViewModel.Dispose();
			}

			Application.Current.Dispatcher.Invoke(() => this.IsEnabled = false);
			this.ViewModel = null;

			if (selection == null || (selection.Type != ActorTypes.Player && selection.Type != ActorTypes.EventNpc))
				return;

			this.ViewModel = new AppearanceViewModel(selection);
			this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = true;

				if (this.ViewModel.Race == 0)
					this.ViewModel.Race = Appearance.Races.Hyur;

				this.Race = this.gameDataService.Races.Get((byte)this.ViewModel.Race);
				this.TribeComboBox.ItemsSource = this.Race.Tribes;
				this.Tribe = this.gameDataService.Tribes.Get((byte)this.ViewModel.Tribe);

				this.OnViewModelPropertyChanged(null, null);
			});
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e == null || e.PropertyName == nameof(AppearanceViewModel.Race) || e.PropertyName == nameof(AppearanceViewModel.Tribe) || e.PropertyName == nameof(AppearanceViewModel.Gender))
			{
				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AppearanceEditor.Race)));
				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AppearanceEditor.Tribe)));

				this.HasFur = this.ViewModel.Race == Appearance.Races.Hrothgar;
				this.HasTail = this.ViewModel.Race == Appearance.Races.Hrothgar || this.ViewModel.Race == Appearance.Races.Miqote || this.ViewModel.Race == Appearance.Races.AuRa;
				this.HasLimbal = this.ViewModel.Race == Appearance.Races.AuRa;
				this.HasEars = this.ViewModel.Race == Appearance.Races.Vierra;
				this.HasMuscles = !this.HasEars && !this.HasTail;
				this.HasGender = this.ViewModel.Race != Appearance.Races.Hrothgar && this.ViewModel.Race != Appearance.Races.Vierra;

				bool canAge = this.ViewModel.Tribe == Appearance.Tribes.Midlander;
				canAge |= this.ViewModel.Race == Appearance.Races.Miqote && this.ViewModel.Gender == Appearance.Genders.Feminine;
				canAge |= this.ViewModel.Race == Appearance.Races.Elezen;
				canAge |= this.ViewModel.Race == Appearance.Races.AuRa && this.ViewModel.Gender == Appearance.Genders.Feminine;
				this.CanAge = canAge;

				this.Hair = this.gameDataService.CharacterMakeCustomize.GetHair(this.ViewModel.Tribe, this.ViewModel.Gender, this.ViewModel.Hair);
			}
			else if (e.PropertyName == nameof(AppearanceViewModel.Hair))
			{
				this.Hair = this.gameDataService.CharacterMakeCustomize.GetHair(this.ViewModel.Tribe, this.ViewModel.Gender, this.ViewModel.Hair);
			}
		}

		private async void OnHairClicked(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();
			HairSelectorDrawer selector = new HairSelectorDrawer(this.ViewModel.Gender, this.ViewModel.Tribe, this.ViewModel.Hair);
			await viewService.ShowDrawer(selector, "Hair");

			if (selector.Selected <= 0)
				return;

			this.ViewModel.Hair = (byte)selector.Selected;
		}
	}
}
