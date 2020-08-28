// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Views
{
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.AppearanceModule.ViewModels;
	using Anamnesis.GameData;
	using Anamnesis.WpfStyles.Drawers;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	public partial class ModelTypeSelector : UserControl, SelectorDrawer.ISelectorView
	{
		private Mode mode;

		public ModelTypeSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;

			IGameDataService gameData = Services.Get<IGameDataService>();

			foreach (ModelTypes item in Module.ModelTypes)
			{
				this.Selector.Items.Add(item);
			}

			this.Selector.FilterItems();
		}

		public event DrawerEvent Close;
		public event DrawerEvent SelectionChanged;

		private enum Mode
		{
			All,

			Characters,
			Mounts,
			Minions,
			Effects,
			Monsters,
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
			if (obj is ModelTypes item)
			{
				if (this.mode != Mode.All)
				{
					if ((int)this.mode != (int)item.Type)
					{
						return false;
					}
				}

				bool matches = false;
				matches |= SearchUtility.Matches(item.Name, search);
				matches |= SearchUtility.Matches(item.Id.ToString(), search);
				return matches;
			}

			return false;
		}

		private void OnModeChanged(object sender, SelectionChangedEventArgs e)
		{
			this.mode = (Mode)this.ModeComboBox.SelectedIndex;
			this.Selector?.FilterItems();
		}
	}
}
