// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.GameData;
	using ConceptMatrix.WpfStyles.Drawers;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	public partial class DyeSelector : UserControl, SelectorDrawer.ISelectorView
	{
		public DyeSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;

			IGameDataService gameData = Services.Get<IGameDataService>();
			this.Selector.Items.Add(EquipmentBaseViewModel.NoneDye);
			foreach (IDye item in gameData.Dyes.All)
			{
				this.Selector.Items.Add(item);
			}

			this.Selector.FilterItems();
		}

		public event DrawerEvent Close;

		public IDye Value
		{
			get
			{
				return (IDye)this.Selector.Value;
			}

			set
			{
				this.Selector.Value = value;
			}
		}

		SelectorDrawer SelectorDrawer.ISelectorView.Selector
		{
			get
			{
				return this.Selector;
			}
		}

		private void OnClose()
		{
			this.Close?.Invoke();
		}

		private bool OnFilter(object obj, string[] search = null)
		{
			if (obj is IDye dye)
			{
				// skip items without names
				if (string.IsNullOrEmpty(dye.Name))
					return false;

				if (!SearchUtility.Matches(dye.Name, search))
					return false;

				return true;
			}

			return false;
		}
	}
}
