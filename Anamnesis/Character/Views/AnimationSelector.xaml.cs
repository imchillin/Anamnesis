// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Excel;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class AnimationSelector : UserControl, SelectorDrawer.ISelectorView, INotifyPropertyChanged
	{
		public AnimationSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;
		public event PropertyChangedEventHandler? PropertyChanged;

		public ActionTimeline? Value
		{
			get
			{
				return (ActionTimeline?)this.Selector.Value;
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
				this.Selector.AddItems(GameDataService.ActionTimelines);

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

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is ActionTimeline action)
			{
				bool matches = false;
				matches |= SearchUtility.Matches(action.Key, search);
				matches |= SearchUtility.Matches(action.RowId.ToString(), search);

				if (string.IsNullOrEmpty(action.Key))
					return false;

				return matches;
			}

			return false;
		}

		private int OnSort(object a, object b)
		{
			if (a == b)
				return 0;

			if (a is ActionTimeline actionA && b is ActionTimeline actionB)
			{
				if (actionA.Key == null)
					return 1;

				if (actionB.Key == null)
					return -1;

				return -actionB.Key.CompareTo(actionA.Key);
			}

			return 0;
		}
	}
}
