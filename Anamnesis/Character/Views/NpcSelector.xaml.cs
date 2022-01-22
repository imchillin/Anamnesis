// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Excel;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class NpcSelector : UserControl, SelectorDrawer.ISelectorView, INotifyPropertyChanged
	{
		public NpcSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;
			this.Filter = GlobalFilter;

			this.Filter.PropertyChanged += this.OnSelfPropertyChanged;
			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;
		public event PropertyChangedEventHandler? PropertyChanged;

		public NpcFilter Filter { get; private set; }

		public INpcBase? Value
		{
			get
			{
				return (INpcBase?)this.Selector.Value;
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

		private static NpcFilter GlobalFilter { get; set; } = new()
		{
			TypesLocked = false,
			IncludeResidentNpc = true,
			IncludeBattleNpc = true,
			IncludeEventNpc = true,
			IncludeMount = true,
			IncludeCompanion = true,
			IncludeOrnament = true,
		};

		public void ChangeFilter(NpcFilter filter)
		{
			this.Filter.PropertyChanged -= this.OnSelfPropertyChanged;
			this.Filter = filter;
			this.Filter.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public void OnClosed()
		{
		}

		private Task OnLoadItems()
		{
			if (GameDataService.ResidentNPCs != null)
				this.Selector.AddItems(GameDataService.ResidentNPCs);

			if (GameDataService.BattleNPCs != null)
				this.Selector.AddItems(GameDataService.BattleNPCs);

			if (GameDataService.EventNPCs != null)
				this.Selector.AddItems(GameDataService.EventNPCs);

			if (GameDataService.Mounts != null)
				this.Selector.AddItems(GameDataService.Mounts);

			if (GameDataService.Companions != null)
				this.Selector.AddItems(GameDataService.Companions);

			if (GameDataService.Ornaments != null)
				this.Selector.AddItems(GameDataService.Ornaments);

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

				// Then Residents
				if (npcA is ResidentNpc && npcB is not ResidentNpc)
					return -1;

				if (npcA is not ResidentNpc && npcB is ResidentNpc)
					return 1;

				// Then Mounts
				if (npcA is Mount && npcB is not Mount)
					return -1;

				if (npcA is not Mount && npcB is Mount)
					return 1;

				// Then Minions
				if (npcA is Companion && npcB is not Companion)
					return -1;

				if (npcA is not Companion && npcB is Companion)
					return 1;

				// Then Battle NPCs
				if (npcA is BattleNpc && npcB is not BattleNpc)
					return -1;

				if (npcA is not BattleNpc && npcB is BattleNpc)
					return 1;

				// Then Event NPCs
				if (npcA is EventNpcAppearance && npcB is not EventNpcAppearance)
					return -1;

				if (npcA is not EventNpcAppearance && npcB is EventNpcAppearance)
					return 1;

				// Then Ornaments
				if (npcA is Ornament && npcB is not Ornament)
					return -1;

				if (npcA is not Ornament && npcB is Ornament)
					return 1;

				return -npcB.RowId.CompareTo(npcA.RowId);
			}

			return 0;
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is INpcBase npc)
			{
				if (this.Filter.IncludeNamed == true && !npc.HasName)
					return false;

				if (this.Filter.IncludeNamed == false && npc.HasName)
					return false;

				if (this.Filter.IncludeModded == true && npc.Mod == null)
					return false;

				if (this.Filter.IncludeModded == false && npc.Mod != null)
					return false;

				if (npc is ResidentNpc)
				{
					if (!this.Filter.IncludeResidentNpc)
						return false;

					// Npc residents without names are useless, since the same
					// npc will just appear in the event npc list anyway.
					if (string.IsNullOrEmpty(npc.Name))
					{
						return false;
					}
				}

				if (npc is Mount)
				{
					if (!this.Filter.IncludeMount)
						return false;

					// Mounts with model Id 0 are just empty entries.
					if (npc.ModelCharaRow == 0)
					{
						return false;
					}
				}

				if (npc is Companion)
				{
					if (!this.Filter.IncludeCompanion)
						return false;

					// Companions with model Id 0 are just empty entries.
					if (npc.ModelCharaRow == 0)
					{
						return false;
					}
				}

				if (npc is Ornament)
				{
					if (!this.Filter.IncludeOrnament)
						return false;

					// Ornaments with model Id 0 are just empty entries.
					if (npc.ModelCharaRow == 0)
					{
						return false;
					}
				}

				if (!this.Filter.IncludeBattleNpc && npc is BattleNpc)
					return false;

				if (!this.Filter.IncludeEventNpc && npc is EventNpc)
					return false;

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

		[AddINotifyPropertyChangedInterface]
		public class NpcFilter : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler? PropertyChanged;

			public bool IncludeResidentNpc { get; set; } = false;
			public bool IncludeBattleNpc { get; set; } = false;
			public bool IncludeEventNpc { get; set; } = false;
			public bool IncludeMount { get; set; } = false;
			public bool IncludeCompanion { get; set; } = false;
			public bool IncludeOrnament { get; set; } = false;
			public bool? IncludeNamed { get; set; } = true;
			public bool? IncludeModded { get; set; } = null;
			public bool TypesLocked { get; set; } = true;
		}
	}
}
