// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Collections.Generic;
	using System.Windows.Controls;
	using ConceptMatrix;
	using ConceptMatrix.GameData;
	using ConceptMatrix.WpfStyles.Drawers;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	public partial class NpcSelector : UserControl, SelectorDrawer.ISelectorView
	{
		public NpcSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;

			IGameDataService gameData = Services.Get<IGameDataService>();

			foreach (INpcResident npc in gameData.ResidentNPCs.All)
			{
				this.Selector.Items.Add(npc);
			}

			this.Selector.FilterItems();
		}

		public event DrawerEvent Close;
		public event DrawerEvent SelectionChanged;

		public INpcResident Value
		{
			get
			{
				return (INpcResident)this.Selector.Value;
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

		private void OnSelectionChanged()
		{
			this.SelectionChanged?.Invoke();
		}

		private bool OnFilter(object obj, string[] search = null)
		{
			if (obj is INpcResident npc)
			{
				if (npc.Appearance == null)
					return false;

				if (npc.Appearance.Race == null)
					return false;

				bool matches = false;

				matches |= SearchUtility.Matches(npc.Singular, search);
				matches |= SearchUtility.Matches(npc.Plural, search);
				matches |= SearchUtility.Matches(npc.Title, search);
				matches |= SearchUtility.Matches(npc.Key.ToString(), search);

				return matches;
			}

			return false;
		}
	}
}
