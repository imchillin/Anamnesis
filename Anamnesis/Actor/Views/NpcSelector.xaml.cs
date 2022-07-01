// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using Anamnesis.Utils;
using PropertyChanged;
using XivToolsWpf;

public partial class NpcSelector : UserControl, INotifyPropertyChanged
{
	public NpcSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;
		this.CurrentFilter = GlobalFilter;

		this.CurrentFilter.PropertyChanged += this.OnSelfPropertyChanged;
		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public NpcFilter CurrentFilter { get; private set; }

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
		this.CurrentFilter.PropertyChanged -= this.OnSelfPropertyChanged;
		this.CurrentFilter = filter;
		this.CurrentFilter.PropertyChanged += this.OnSelfPropertyChanged;
	}

	protected Task LoadItems()
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

	protected int Compare(INpcBase itemA, INpcBase itemB)
	{
		// Faorites to the top
		if (itemA.IsFavorite && !itemB.IsFavorite)
			return -1;

		if (!itemA.IsFavorite && itemB.IsFavorite)
			return 1;

		// Then Residents
		if (itemA is ResidentNpc && itemB is not ResidentNpc)
			return -1;

		if (itemA is not ResidentNpc && itemB is ResidentNpc)
			return 1;

		// Then Mounts
		if (itemA is Mount && itemB is not Mount)
			return -1;

		if (itemA is not Mount && itemB is Mount)
			return 1;

		// Then Minions
		if (itemA is Companion && itemB is not Companion)
			return -1;

		if (itemA is not Companion && itemB is Companion)
			return 1;

		// Then Battle NPCs
		if (itemA is BattleNpc && itemB is not BattleNpc)
			return -1;

		if (itemA is not BattleNpc && itemB is BattleNpc)
			return 1;

		// Then Event NPCs
		if (itemA is EventNpcAppearance && itemB is not EventNpcAppearance)
			return -1;

		if (itemA is not EventNpcAppearance && itemB is EventNpcAppearance)
			return 1;

		// Then Ornaments
		if (itemA is Ornament && itemB is not Ornament)
			return -1;

		if (itemA is not Ornament && itemB is Ornament)
			return 1;

		return -itemB.RowId.CompareTo(itemA.RowId);
	}

	protected bool Filter(INpcBase npc, string[]? search)
	{
		if (this.CurrentFilter.IncludeNamed == true && !npc.HasName)
			return false;

		if (this.CurrentFilter.IncludeNamed == false && npc.HasName)
			return false;

		if (this.CurrentFilter.IncludeModded == true && npc.Mod == null)
			return false;

		if (this.CurrentFilter.IncludeModded == false && npc.Mod != null)
			return false;

		if (npc is ResidentNpc)
		{
			if (!this.CurrentFilter.IncludeResidentNpc)
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
			if (!this.CurrentFilter.IncludeMount)
				return false;

			// Mounts with model Id 0 are just empty entries.
			if (npc.ModelCharaRow == 0)
			{
				return false;
			}
		}

		if (npc is Companion)
		{
			if (!this.CurrentFilter.IncludeCompanion)
				return false;

			// Companions with model Id 0 are just empty entries.
			if (npc.ModelCharaRow == 0)
			{
				return false;
			}
		}

		if (npc is Ornament)
		{
			if (!this.CurrentFilter.IncludeOrnament)
				return false;

			// Ornaments with model Id 0 are just empty entries.
			if (npc.ModelCharaRow == 0)
			{
				return false;
			}
		}

		if (!this.CurrentFilter.IncludeBattleNpc && npc is BattleNpc)
			return false;

		if (!this.CurrentFilter.IncludeEventNpc && npc is EventNpc)
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

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.CurrentFilter))
			return;

		this.Selector.FilterItems();
	}

	private async void OnCopyId(object sender, RoutedEventArgs e)
	{
		if (this.Selector.Value == null)
			return;

		if (this.Selector.Value is INpcBase npc)
		{
			await ClipboardUtility.CopyToClipboardAsync(npc.ToStringKey());
		}
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
