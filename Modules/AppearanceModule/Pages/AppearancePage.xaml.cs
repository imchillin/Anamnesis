// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Utilities;
	using ConceptMatrix.AppearanceModule.Views;
	using ConceptMatrix.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for AppearancePage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AppearancePage : UserControl
	{
		private IRace race;

		public AppearancePage()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			IGameDataService gameDataService = Module.Services.Get<IGameDataService>();

			this.GenderComboBox.ItemsSource = new[] { Appearance.Genders.Masculine, Appearance.Genders.Feminine };
			this.RaceComboBox.ItemsSource = gameDataService.Races.All;
			this.TribeComboBox.ItemsSource = gameDataService.Tribes.All;

			ColorData.GetSkin(Appearance.Tribes.Highlander, Appearance.Genders.Feminine);

			this.Gender = Appearance.Genders.Feminine;
		}

		[DependsOn(nameof(Race))]
		public bool HasFur
		{
			get
			{
				if (this.Race == null)
					return false;

				return this.Race.Race == Appearance.Races.Hrothgar;
			}
		}

		[DependsOn(nameof(Race))]
		public bool HasLimbal
		{
			get
			{
				if (this.Race == null)
					return false;

				return this.Race.Race == Appearance.Races.AuRa;
			}
		}

		[DependsOn(nameof(Race))]
		public bool HasTail
		{
			get
			{
				if (this.Race == null)
					return false;

				return this.Race.Race == Appearance.Races.Hrothgar || this.Race.Race == Appearance.Races.Miqote || this.Race.Race == Appearance.Races.AuRa;
			}
		}

		[DependsOn(nameof(Race))]
		public bool HasEars
		{
			get
			{
				if (this.Race == null)
					return false;

				return this.race.Race == Appearance.Races.Vierra;
			}
		}

		[DependsOn(nameof(Race))]
		public bool HasMuscles
		{
			get
			{
				return !this.HasEars && !this.HasTail;
			}
		}

		public Appearance.Genders Gender
		{
			get;
			set;
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

		[DependsOn(nameof(Race))]
		public ITribe Tribe
		{
			get;
			set;
		}

		public byte Skintone { get; set; }
		public byte EyeColorL { get; set; }
		public byte EyeColorR { get; set; }
		public byte FacePaintColor { get; set; }
		public byte LimbalTattooColor { get; set; }
		public byte HairColor { get; set; }
		public byte HairHighlights { get; set; }
		public byte LipsColor { get; set; }
	}
}
