// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	/// <summary>
	/// Interaction logic for HairSelector.xaml.
	/// </summary>
	public partial class HairSelectorDrawer : UserControl, IDrawer
	{
		private byte selected;

		public HairSelectorDrawer(Appearance.Genders gender, Appearance.Tribes tribe, byte value)
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			this.selected = value;

			this.List.ItemsSource = GameDataService.CharacterMakeCustomize?.GetHair(tribe, gender);
			this.List.SelectedItem = GameDataService.CharacterMakeCustomize?.GetHair(tribe, gender, value);
		}

		public delegate void SelectorEvent(byte value);

		public event DrawerEvent? Close;
		public event SelectorEvent? SelectionChanged;

		public byte Selected
		{
			get
			{
				return this.selected;
			}

			set
			{
				this.selected = value;
				this.SelectionChanged?.Invoke(this.selected);
			}
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			ICharaMakeCustomize? hair = this.List.SelectedItem as ICharaMakeCustomize;

			if (hair == null)
				return;

			this.Selected = hair.FeatureId;
		}
	}
}
