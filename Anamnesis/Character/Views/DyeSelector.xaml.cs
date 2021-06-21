// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Collections.Generic;
	using System.Windows.Controls;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	public partial class DyeSelector : UserControl, SelectorDrawer.ISelectorView
	{
		public DyeSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;

			List<IDye> alldyes = new List<IDye>();
			alldyes.Add(DyeUtility.NoneDye);

			if (GameDataService.Dyes != null)
				alldyes.AddRange(GameDataService.Dyes);

			alldyes.Sort((a, b) => b.IsFavorite.CompareTo(a.IsFavorite));

			this.Selector.AddItems(alldyes);
			this.Selector.FilterItems();
		}

		public event DrawerEvent? Close;

		public IDye? Value
		{
			get
			{
				return (IDye?)this.Selector.Value;
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

		public void OnClosed()
		{
		}

		private void OnClose()
		{
			this.Close?.Invoke();
		}

		private bool OnFilter(object obj, string[]? search = null)
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
