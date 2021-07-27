// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.GameData;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class NpcSelector : UserControl, SelectorDrawer.ISelectorView, INotifyPropertyChanged
	{
		private static bool includeNpc = true;
		private static bool includeCharacter = true;
		private static bool includeMount = true;
		private static bool includeMinion = true;
		private static bool includeEffect = true;
		private static bool includeMonster = true;
		private static bool includeUnknown = true;
		private static bool? includeModded = null;
		private static bool? includeNamed = true;

		public NpcSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;

			List<INpcResident> allNpcs = new List<INpcResident>();
			List<Monster> allMonsters = new List<Monster>();

			if (GameDataService.ResidentNPCs != null)
				allNpcs.AddRange(GameDataService.ResidentNPCs);

			if (GameDataService.Monsters != null)
				allMonsters.AddRange(GameDataService.Monsters);

			allNpcs.Sort((a, b) => b.IsFavorite.CompareTo(a.IsFavorite));
			allMonsters.Sort((a, b) => b.IsFavorite.CompareTo(a.IsFavorite));

			this.Selector.AddItems(allNpcs);
			this.Selector.AddItems(allMonsters);

			this.Selector.FilterItems();

			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;
		public event PropertyChangedEventHandler? PropertyChanged;

		public bool IncludeNpc { get => includeNpc; set => includeNpc = value; }
		public bool IncludeCharacter { get => includeCharacter; set => includeCharacter = value; }
		public bool IncludeMount { get => includeMount; set => includeMount = value; }
		public bool IncludeMinion { get => includeMinion; set => includeMinion = value; }
		public bool IncludeEffect { get => includeEffect; set => includeEffect = value; }
		public bool IncludeMonster { get => includeMonster; set => includeMonster = value; }
		public bool IncludeUnknown { get => includeUnknown; set => includeUnknown = value; }
		public bool? IncludeModded { get => includeModded; set => includeModded = value; }
		public bool? IncludeNamed { get => includeNamed; set => includeNamed = value; }

		public INpcResident? Value
		{
			get
			{
				return (INpcResident?)this.Selector.Value;
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

		private void OnSelectionChanged()
		{
			this.SelectionChanged?.Invoke();
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is INpcResident npc)
			{
				if (npc.Appearance == null || npc.Appearance.Race == null || npc.Appearance.Race.Key == 0)
					return false;

				if (!this.IncludeNpc && !(obj is Monster))
					return false;

				if (obj is Monster mon)
				{
					if (!this.IncludeCharacter && mon.Type == Monster.Types.Character)
						return false;

					if (!this.IncludeEffect && mon.Type == Monster.Types.Effect)
						return false;

					if (!this.IncludeMinion && mon.Type == Monster.Types.Minion)
						return false;

					if (!this.IncludeMonster && mon.Type == Monster.Types.Monster)
						return false;

					if (!this.IncludeMount && mon.Type == Monster.Types.Mount)
						return false;

					if (!this.IncludeUnknown && mon.Type == Monster.Types.Unknown)
						return false;
				}

				if (this.IncludeModded == true && npc.Mod == null)
					return false;

				if (this.IncludeModded == false && npc.Mod != null)
					return false;

				if (this.IncludeNamed == true && string.IsNullOrEmpty(npc.Name))
					return false;

				if (this.IncludeNamed == false && !string.IsNullOrEmpty(npc.Name))
					return false;

				bool matches = false;
				matches |= SearchUtility.Matches(npc.Singular, search);
				matches |= SearchUtility.Matches(npc.Plural, search);
				matches |= SearchUtility.Matches(npc.Title, search);
				matches |= SearchUtility.Matches(npc.Key.ToString(), search);

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
