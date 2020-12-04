// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;
	using Anamnesis.Services;
	using Anamnesis.Styles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class EquipmentSelector : UserControl, SelectorDrawer.ISelectorView
	{
		private static Mode mode = Mode.All;
		private static Classes classFilter = Classes.All;

		private readonly ItemSlots slot;

		public EquipmentSelector(ItemSlots slot)
		{
			this.slot = slot;

			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.Selector.AddItem(ItemUtility.NoneItem);
			this.Selector.AddItem(ItemUtility.NpcbodyItem);

			// Special case for hands to also list props
			if (GameDataService.Props != null)
			{
				if (slot == ItemSlots.MainHand || slot == ItemSlots.OffHand)
				{
					this.Selector.AddItems(GameDataService.Props);
				}
			}

			if (GameDataService.Items != null)
			{
				this.Selector.AddItems(GameDataService.Items);
			}

			this.JobFilterText.Text = classFilter.Describe();
			this.Selector.FilterItems();
		}

		public event DrawerEvent? Close;
		public event DrawerEvent? SelectionChanged;

		private enum Mode
		{
			All,
			Items,
			Props,
			Modded,
		}

		public IItem? Value
		{
			get
			{
				return (IItem?)this.Selector.Value;
			}

			set
			{
				this.Selector.Value = value;
			}
		}

		public bool AllMode
		{
			get => mode == Mode.All;
			set
			{
				if (value)
				{
					mode = Mode.All;
					this.Selector.FilterItems();
				}
			}
		}

		public bool PropsMode
		{
			get => mode == Mode.Props;
			set
			{
				if (value)
				{
					mode = Mode.Props;
					this.Selector.FilterItems();
				}
			}
		}

		public bool ItemsMode
		{
			get => mode == Mode.Items;
			set
			{
				if (value)
				{
					mode = Mode.Items;
					this.Selector.FilterItems();
				}
			}
		}

		public bool ModdedMode
		{
			get => mode == Mode.Modded;
			set
			{
				if (value)
				{
					mode = Mode.Modded;
					this.Selector.FilterItems();
				}
			}
		}

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
			if (obj is IItem item)
			{
				// skip items without names
				if (string.IsNullOrEmpty(item.Name))
					return false;

				if (mode == Mode.Items && (obj is Prop || item.Key == 0))
					return false;

				if (mode == Mode.Props && !(obj is Prop))
					return false;

				if (mode == Mode.Modded && item.Mod == null)
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
