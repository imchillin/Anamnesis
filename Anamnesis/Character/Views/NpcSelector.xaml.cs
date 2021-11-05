// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.GameData;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class NpcSelector : UserControl, SelectorDrawer.ISelectorView, INotifyPropertyChanged
	{
		private static bool includeResidentNpc = true;
		private static bool includeBattleNpc = true;
		private static bool includeEventNpc = true;
		private static bool includeMount = true;
		private static bool? includeModded = null;

		public NpcSelector()
		{
			this.InitializeComponent();
			this.DataContext = this;

			if (GameDataService.ResidentNPCs != null)
				this.Selector.AddItems(GameDataService.ResidentNPCs);

			if (GameDataService.BattleNPCs != null)
				this.Selector.AddItems(GameDataService.BattleNPCs);

			if (GameDataService.EventNPCs != null)
				this.Selector.AddItems(GameDataService.EventNPCs);

			if (GameDataService.Mounts != null)
				this.Selector.AddItems(GameDataService.Mounts);

			this.Selector.FilterItems();

			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;
		public event PropertyChangedEventHandler? PropertyChanged;

		public bool IncludeResidentNpc
		{
			get => includeResidentNpc;
			set => includeResidentNpc = value;
		}

		public bool IncludeBattleNpc
		{
			get => includeBattleNpc;
			set => includeBattleNpc = value;
		}

		public bool IncludeEventNpc
		{
			get => includeEventNpc;
			set => includeEventNpc = value;
		}

		public bool IncludeMount
		{
			get => includeMount;
			set => includeMount = value;
		}

		public bool? IncludeModded
		{
			get => includeModded;
			set => includeModded = value;
		}

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
			if (obj is INpcBase npc)
			{
				if (this.IncludeModded == true && npc.Mod == null)
					return false;

				if (this.IncludeModded == false && npc.Mod != null)
					return false;

				if (npc is NpcResidentViewModel)
				{
					if (!this.IncludeResidentNpc)
						return false;

					// Npc residents without names are useless, since the same
					// npc will just appear in the event npc list anyway.
					if (string.IsNullOrEmpty(npc.Name))
					{
						return false;
					}
				}

				if (!this.IncludeBattleNpc && npc is BNpcBaseViewModel)
					return false;

				if (!this.IncludeEventNpc && npc is ENpcBaseViewModel)
					return false;

				if (npc is MountViewModel)
				{
					if (!this.IncludeMount)
						return false;

					// Mounts with model Id 0 are just empty entries.
					if (npc.ModelCharaRow == 0)
					{
						return false;
					}
				}

				bool matches = false;
				matches |= SearchUtility.Matches(npc.Name, search);
				matches |= SearchUtility.Matches(npc.Key.ToString(), search);
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
