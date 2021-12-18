// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ModelListSelector : UserControl, SelectorDrawer.ISelectorView, INotifyPropertyChanged
	{
		public ModelListSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;

			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;
		public event PropertyChangedEventHandler? PropertyChanged;

		public ModelListEntry? Value
		{
			get
			{
				return (ModelListEntry?)this.Selector.Value;
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

		private Task OnLoadItems()
		{
			if (GameDataService.ModelList != null)
				this.Selector.AddItems(GameDataService.ModelList);

			return Task.CompletedTask;
		}

		private void OnClose()
		{
			this.Close?.Invoke();
		}

		private void OnSelectionChanged()
		{
			this.SelectionChanged?.Invoke();
		}

		private int OnSort(object a, object b)
		{
			if (a == b)
				return 0;

			if (a is INpcBase npcA && b is INpcBase npcB)
			{
				// Faorites to the top
				if (npcA.IsFavorite && !npcB.IsFavorite)
					return -1;

				if (!npcA.IsFavorite && npcB.IsFavorite)
					return 1;

				return -npcB.RowId.CompareTo(npcA.RowId);
			}

			return 0;
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is INpcBase npc)
			{
				bool matches = false;
				matches |= SearchUtility.Matches(npc.Name, search);
				matches |= SearchUtility.Matches(npc.RowId.ToString(), search);
				matches |= SearchUtility.Matches(npc.ModelCharaRow.ToString(), search);

				if (npc.Mod != null && npc.Mod.ModPack != null)
				{
					matches |= SearchUtility.Matches(npc.Mod.ModPack.Name, search);
				}

				return matches;
			}

			return false;
		}

		private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			this.Selector.FilterItems();
		}
	}
}
