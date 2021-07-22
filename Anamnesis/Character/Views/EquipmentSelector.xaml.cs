// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Serialization;
	using Anamnesis.Services;
	using Anamnesis.Styles.Controls;
	using Anamnesis.Styles.Drawers;
	using Lumina.Excel.GeneratedSheets;
	using PropertyChanged;
	using static Anamnesis.Memory.Customize;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class EquipmentSelector : UserControl, SelectorDrawer.ISelectorView
	{
		private static readonly Dictionary<uint, ItemCategories> ManualItemCategories;

		private static Classes classFilter = Classes.All;
		private static ItemCategories categoryFilter = ItemCategories.All;
		private static bool hideLocked = true;
		private static bool ambidextrous = false;
		private static bool autoOffhand = true;
		private static bool showFilters = false;

		private readonly ItemSlots slot;
		private readonly Memory.ActorViewModel? actor;

		static EquipmentSelector()
		{
			ManualItemCategories = SerializerService.DeserializeFile<Dictionary<uint, ItemCategories>>("Data/ItemCategories.json");
		}

		public EquipmentSelector(ItemSlots slot, Memory.ActorViewModel? actor)
		{
			this.slot = slot;
			this.actor = actor;

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

		public IItem? Value
		{
			get => (IItem?)this.Selector.Value;
			set => this.Selector.Value = value;
		}

		public bool ShowFilters
		{
			get => showFilters;
			set => showFilters = value;
		}

		public bool IsMainHandSlot => this.slot == ItemSlots.MainHand;
		public bool IsWeaponSlot => this.slot == ItemSlots.MainHand || this.slot == ItemSlots.OffHand;

		SelectorDrawer SelectorDrawer.ISelectorView.Selector => this.Selector;

		public Classes ClassFilter
		{
			get => classFilter;
			set
			{
				classFilter = value;
				this.JobFilterText.Text = value.Describe();
				this.Selector.FilterItems();
			}
		}

		public ItemCategories CategoryFilter
		{
			get => categoryFilter;
			set
			{
				categoryFilter = value;
				this.Selector.FilterItems();
			}
		}

		public bool HideLocked
		{
			get => hideLocked;
			set
			{
				hideLocked = value;
				this.Selector.FilterItems();
			}
		}

		public bool Ambidextrous
		{
			get => ambidextrous;
			set
			{
				ambidextrous = value;
				this.Selector.FilterItems();
			}
		}

		public bool AutoOffhand
		{
			get => autoOffhand;
			set => autoOffhand = value;
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
			if (obj is not IItem item)
				return false;

			// skip items without names
			if (string.IsNullOrEmpty(item.Name))
				return false;

			if (this.Ambidextrous && (this.slot == ItemSlots.MainHand || this.slot == ItemSlots.OffHand))
			{
				if (!item.IsWeapon)
					return false;
			}
			else
			{
				if (!item.FitsInSlot(this.slot))
					return false;
			}

			if (!this.HasClass(this.ClassFilter, item.EquipableClasses))
				return false;

			if (!this.ValidCategory(item))
				return false;

			if (this.HideLocked && item is ItemViewModel ivm && !this.CanEquip(ivm))
				return false;

			return this.MatchesSearch(item, search);
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

		private bool ValidCategory(IItem item)
		{
			ItemCategories itemCategory = ItemCategories.None;
			if (item is ItemViewModel)
				itemCategory = ManualItemCategories.GetValueOrDefault(item.Key, ItemCategories.Standard);
			else if (item is Prop)
				itemCategory = ItemCategories.Props;
			else if (item is PerformViewModel)
				itemCategory = ItemCategories.Performance;

			// Always let uncategorized items through, otherwise there would be no way to get to them
			bool categoryFiltered = itemCategory == ItemCategories.None;
			categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Standard) && itemCategory.HasFlag(ItemCategories.Standard);
			categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Premium) && itemCategory.HasFlag(ItemCategories.Premium);
			categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Limited) && itemCategory.HasFlag(ItemCategories.Limited);
			categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Deprecated) && itemCategory.HasFlag(ItemCategories.Deprecated);
			categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Props) && itemCategory.HasFlag(ItemCategories.Props);
			categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Performance) && itemCategory.HasFlag(ItemCategories.Performance);
			categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Modded) && item.Mod != null;
			categoryFiltered |= this.CategoryFilter.HasFlag(ItemCategories.Favorites) && item.IsFavorite;
			return categoryFiltered;
		}

		private bool CanEquip(ItemViewModel item)
		{
			EquipRaceCategory? equipRaceCategory;
			lock (GameDataService.EquipRaceCategories)
				equipRaceCategory = GameDataService.EquipRaceCategories.GetRow(item.Value.EquipRestriction);

			if (equipRaceCategory == null || this.actor == null || this.actor.Customize == null)
				return true;

			Genders gender = this.actor.Customize.Gender;
			bool validGender = (gender == Genders.Masculine && equipRaceCategory.Male)
				|| (gender == Genders.Feminine && equipRaceCategory.Female);

			Races race = this.actor.Customize.Race;
			bool validRace = (race == Races.Hyur && equipRaceCategory.Hyur)
				|| (race == Races.Elezen && equipRaceCategory.Elezen)
				|| (race == Races.Lalafel && equipRaceCategory.Lalafell)
				|| (race == Races.Miqote && equipRaceCategory.Miqote)
				|| (race == Races.Roegadyn && equipRaceCategory.Roegadyn)
				|| (race == Races.AuRa && equipRaceCategory.AuRa)
				|| (race == Races.Hrothgar && equipRaceCategory.Unknown6)
				|| (race == Races.Viera && equipRaceCategory.Unknown7);

			return validGender && validRace;
		}

		private bool MatchesSearch(IItem item, string[]? search = null)
		{
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
	}
}
