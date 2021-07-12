// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for HairSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class CustomizeFeatureSelectorDrawer : UserControl, IDrawer
	{
		private readonly Customize.Genders gender;
		private readonly Customize.Tribes tribe;
		private readonly Features feature;

		private byte selected;
		private ICharaMakeCustomize? selectedItem;

		public CustomizeFeatureSelectorDrawer(Features feature, Customize.Genders gender, Customize.Tribes tribe, byte value)
		{
			this.InitializeComponent();

			this.feature = feature;
			this.gender = gender;
			this.tribe = tribe;

			this.ContentArea.DataContext = this;
			this.List.ItemsSource = GameDataService.CharacterMakeCustomize?.GetFeatureOptions(feature, tribe, gender);

			this.Selected = value;
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

				if (!this.IsLoaded)
					return;

				this.SelectedItem = GameDataService.CharacterMakeCustomize?.GetFeature(this.feature, this.tribe, this.gender, value);
				this.SelectionChanged?.Invoke(this.selected);
			}
		}

		public ICharaMakeCustomize? SelectedItem
		{
			get
			{
				return this.selectedItem;
			}

			set
			{
				if (this.selectedItem == value)
					return;

				this.selectedItem = value;

				if (value == null)
					return;

				this.Selected = value.FeatureId;
			}
		}

		public void OnClosed()
		{
		}
	}
}
