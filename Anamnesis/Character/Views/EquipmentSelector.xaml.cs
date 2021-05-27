// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class EquipmentSelector : UserControl, SelectorDrawer.ISelectorView
	{
		private static Modes mode = Modes.All;
		private static Classes classFilter = Classes.All;
		private static bool pairEquip = false;

		private readonly ItemSlots slot;

		public EquipmentSelector(ItemSlots slot)
		{
			this.slot = slot;

			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.Selector.AddItem(ItemUtility.NoneItem);
			this.Selector.AddItem(ItemUtility.NpcBodyItem);
			this.Selector.AddItem(ItemUtility.InvisibileBodyItem);
			this.Selector.AddItem(ItemUtility.InvisibileHeadItem);

			// Special case for hands to also list props
			if (GameDataService.Props != null)
				this.Selector.AddItems(GameDataService.Props);

			if (GameDataService.Items != null)
				this.Selector.AddItems(GameDataService.Items);

			if (GameDataService.Perform != null)
				this.Selector.AddItems(GameDataService.Perform);

			this.JobFilterText.Text = classFilter.Describe();
			this.Selector.FilterItems();
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;

		public enum Modes
		{
			All,
			Items,
			Props,
			Performance,
			Modded,
			Favorites,
		}

		public IItem? Value
		{
			get => (IItem?)this.Selector.Value;
			set => this.Selector.Value = value;
		}

		public int ModeInt
		{
			get => (int)mode;
			set
			{
				mode = (Modes)value;
				this.Selector.FilterItems();
			}
		}

		public bool PairEquip
		{
			get => pairEquip;
			set => pairEquip = value;
		}

		public bool IsWeaponSlot => this.slot == ItemSlots.MainHand || this.slot == ItemSlots.OffHand;

		SelectorDrawer SelectorDrawer.ISelectorView.Selector
		{
			get
			{
				return this.Selector;
			}
		}

		public Classes ClassFilter
		{
			get
			{
				return classFilter;
			}
			set
			{
				classFilter = value;
				this.JobFilterText.Text = value.Describe();
				this.Selector.FilterItems();
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

		private int OnSort(object a, object b)
		{
			if (a is IItem itemA && b is IItem itemB)
			{
				if (itemA.IsFavorite && !itemB.IsFavorite)
				{
					return -1;
				}
				else if (!itemA.IsFavorite && itemB.IsFavorite)
				{
					return 1;
				}

				return itemA.Key.CompareTo(itemB.Key);
			}

			return 0;
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is IItem item)
			{
				// skip items without names
				if (string.IsNullOrEmpty(item.Name))
					return false;

				if (mode == Modes.Items && (obj is Prop || item.Key == 0))
					return false;

				if (mode == Modes.Props && !(obj is Prop))
					return false;

				if (mode == Modes.Performance && !(obj is PerformViewModel))
					return false;

				if (mode == Modes.Modded && item.Mod == null)
					return false;

				if (mode == Modes.Favorites && !item.IsFavorite)
					return false;

				if (this.slot == ItemSlots.MainHand || this.slot == ItemSlots.OffHand)
				{
					if (!item.IsWeapon)
					{
						return false;
					}
				}
				else
				{
					if (!item.FitsInSlot(this.slot))
					{
						return false;
					}
				}

				if (!this.HasClass(this.ClassFilter, item.EquipableClasses))
					return false;

				bool matches = false;

				matches |= SearchUtility.Matches(item.Name, search);
				matches |= SearchUtility.Matches(item.Description, search);
				matches |= SearchUtility.Matches(item.ModelSet.ToString(), search);
				matches |= SearchUtility.Matches(item.ModelBase.ToString(), search);
				matches |= SearchUtility.Matches(item.ModelVariant.ToString(), search);

				if (item.HasSubModel)
				{
					matches |= SearchUtility.Matches(item.SubModelSet.ToString(), search);
					matches |= SearchUtility.Matches(item.SubModelBase.ToString(), search);
					matches |= SearchUtility.Matches(item.SubModelVariant.ToString(), search);
				}

				matches |= SearchUtility.Matches(item.Key.ToString(), search);

				if (item.Mod != null && item.Mod.ModPack != null)
				{
					matches |= SearchUtility.Matches(item.Mod.ModPack.Name, search);
				}

				return matches;
			}

			return false;
		}

		private bool HasClass(Classes a, Classes b)
		{
			foreach (Classes? job in Enum.GetValues(typeof(Classes)))
			{
				if (job == null || job == Classes.None)
					continue;

				if (a.HasFlag(job) && b.HasFlag(job))
				{
					return true;
				}
			}

			return false;
		}
	}
}
