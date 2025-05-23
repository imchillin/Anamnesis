﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using Anamnesis.Utils;
using PropertyChanged;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;

public abstract class NpcSelectorDrawer : SelectorDrawer<INpcBase>
{
}

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
public partial class NpcSelector : NpcSelectorDrawer
{
	public NpcSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;
		this.CurrentFilter = GlobalFilter;

		this.CurrentFilter.PropertyChanged += this.OnSelfPropertyChanged;
		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

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

	protected override Task LoadItems()
	{
		if (GameDataService.ResidentNPCs != null)
			this.AddItems(GameDataService.ResidentNPCs.ToEnumerable());

		if (GameDataService.BattleNPCs != null)
			this.AddItems(GameDataService.BattleNPCs.ToEnumerable());

		if (GameDataService.EventNPCs != null)
			this.AddItems(GameDataService.EventNPCs.ToEnumerable());

		if (GameDataService.Mounts != null)
			this.AddItems(GameDataService.Mounts.ToEnumerable());

		if (GameDataService.Companions != null)
			this.AddItems(GameDataService.Companions.ToEnumerable());

		if (GameDataService.Ornaments != null)
			this.AddItems(GameDataService.Ornaments.ToEnumerable());

		return Task.CompletedTask;
	}

	protected override int Compare(INpcBase itemA, INpcBase itemB)
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

		// Then Ornaments
		if (itemA is Ornament && itemB is not Ornament)
			return -1;

		if (itemA is not Ornament && itemB is Ornament)
			return 1;

		return -itemB.RowId.CompareTo(itemA.RowId);
	}

	protected override bool Filter(INpcBase npc, string[]? search)
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

		this.FilterItems();
	}

	private async void OnCopyId(object sender, RoutedEventArgs e)
	{
		if (this.Value == null)
			return;

		await ClipboardUtility.CopyToClipboardAsync(this.Value.ToStringKey());
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
