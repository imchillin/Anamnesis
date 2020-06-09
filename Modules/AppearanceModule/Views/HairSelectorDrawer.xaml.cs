// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Windows.Controls;
	using Anamnesis;
	using ConceptMatrix.GameData;

	/// <summary>
	/// Interaction logic for HairSelector.xaml.
	/// </summary>
	public partial class HairSelectorDrawer : UserControl, IDrawer
	{
		public HairSelectorDrawer(Appearance.Genders gender, Appearance.Tribes tribe, byte value)
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			IGameDataService gameData = Services.Get<IGameDataService>();

			this.List.ItemsSource = gameData.CharacterMakeCustomize.GetHair(tribe, gender);
			this.List.SelectedItem = gameData.CharacterMakeCustomize.GetHair(tribe, gender, value);
		}

		public delegate void SelectorEvent(byte value);

		public event DrawerEvent Close;
		public event SelectorEvent SelectionChanged;

		public ICharaMakeCustomize SelectedItem { get; set; }

		public byte Selected
		{
			get
			{
				if (this.SelectedItem == null)
					return 0;

				return this.SelectedItem.FeatureId;
			}
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			if (this.SelectedItem == null)
				return;

			this.SelectionChanged?.Invoke(this.Selected);
		}
	}
}
