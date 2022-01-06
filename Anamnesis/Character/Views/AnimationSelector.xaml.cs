// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.GameData.Excel;
	using Anamnesis.GameData.Interfaces;
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

		public IAnimation? Value
		{
			get
			{
				return (IAnimation?)this.Selector.Value;
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
			if (GameDataService.Emotes != null)
				this.Selector.AddItems(GameDataService.Emotes);

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
			if (obj is IAnimation animation)
			{
				bool matches = false;
				matches |= SearchUtility.Matches(animation.Name, search);
				matches |= SearchUtility.Matches(animation.ActionTimelineRowId.ToString(), search);

				// Filter out actions without keys
				if (string.IsNullOrEmpty(animation.Name))
					return false;

				return matches;
			}

			return false;
		}

		private int OnSort(object a, object b)
		{
			if (a == b)
				return 0;

			if (a is IAnimation animA && b is IAnimation animB)
			{
				// Emotes to the top
				if (animA is Emote && animB is not Emote)
					return -1;

				if (animA is not Emote && animB is Emote)
					return 1;

				if (animA.Name == null)
					return 1;

				if (animB.Name == null)
					return -1;

				return -animB.Name.CompareTo(animA.Name);
			}

			return 0;
		}
	}
}
