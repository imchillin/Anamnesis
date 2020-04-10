// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System.Collections.Generic;
	using System.Windows.Controls;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	public partial class AppearancePage : UserControl
	{
		private IRace race;

		public AppearancePage()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			IGameDataService gameDataService = Module.Services.Get<IGameDataService>();

			this.RaceComboBox.ItemsSource = gameDataService.Races.All;
			this.TribeComboBox.ItemsSource = gameDataService.Tribes.All;
		}

		public IRace Race
		{
			get
			{
				return this.race;
			}

			set
			{
				this.race = value;
				this.TribeComboBox.ItemsSource = this.race.Tribes;
				this.Tribe = this.race.Tribes.First();
			}
		}

		public ITribe Tribe { get; set; }
	}
}
