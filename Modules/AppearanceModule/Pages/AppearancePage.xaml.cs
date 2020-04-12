// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	public partial class AppearancePage : UserControl, INotifyPropertyChanged
	{
		private IGameDataService gameDataService;

		public AppearancePage()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.gameDataService = Module.Services.Get<IGameDataService>();
			ISelectionService selectionService = Module.Services.Get<ISelectionService>();

			this.GenderComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Genders));
			this.RaceComboBox.ItemsSource = this.gameDataService.Races.All;
			this.AgeComboBox.ItemsSource = Enum.GetValues(typeof(Appearance.Ages));

			selectionService.SelectionChanged += this.OnSelectionChanged;
			this.OnSelectionChanged(selectionService.CurrentSelection);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool HasFur { get; set; }
		public bool HasLimbal { get; set; }
		public bool HasTail { get; set; }
		public bool HasEars { get; set; }
		public bool HasMuscles { get; set; }
		public bool CanAge { get; set; }

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
				this.ViewModel.SubPropertyChanged -= this.OnViewModelPropertyChanged;
				this.ViewModel.Dispose();
			}

			Application.Current.Dispatcher.Invoke(() => this.IsEnabled = false);
			this.ViewModel = null;

			if (selection == null)
				return;

			this.ViewModel = new AppearanceViewModel(selection);
			this.ViewModel.SubPropertyChanged += this.OnViewModelPropertyChanged;
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = true;

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
				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AppearancePage.Race)));
				this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AppearancePage.Tribe)));

				this.HasFur = this.ViewModel.Race == Appearance.Races.Hrothgar;
				this.HasTail = this.ViewModel.Race == Appearance.Races.Hrothgar || this.ViewModel.Race == Appearance.Races.Miqote || this.ViewModel.Race == Appearance.Races.AuRa;
				this.HasLimbal = this.ViewModel.Race == Appearance.Races.AuRa;
				this.HasEars = this.ViewModel.Race == Appearance.Races.Vierra;
				this.HasMuscles = !this.HasEars && !this.HasTail;

				bool canAge = this.ViewModel.Tribe == Appearance.Tribes.Midlander;
				canAge |= this.ViewModel.Race == Appearance.Races.Miqote && this.ViewModel.Gender == Appearance.Genders.Feminine;
				canAge |= this.ViewModel.Race == Appearance.Races.Elezen;
				canAge |= this.ViewModel.Race == Appearance.Races.AuRa && this.ViewModel.Gender == Appearance.Genders.Feminine;
				this.CanAge = canAge;
			}
		}
	}
}
