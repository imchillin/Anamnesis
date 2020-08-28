// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Views
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using Anamnesis.AppearanceModule.ViewModels;
	using Anamnesis.GameData;
	using Anamnesis.WpfStyles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class EquipmentSelector : UserControl, SelectorDrawer.ISelectorView
	{
		private readonly ItemSlots slot;
		private Mode mode;
		private Classes classFilter;

		public EquipmentSelector(ItemSlots slot)
		{
			this.slot = slot;

			this.InitializeComponent();
			this.DataContext = this;

			IGameDataService gameData = Services.Get<IGameDataService>();
			this.Selector.Items.Add(EquipmentBaseViewModel.NoneItem);
			this.Selector.Items.Add(EquipmentBaseViewModel.NpcbodyItem);

			// Special case for hands to also list props
			if (slot == ItemSlots.MainHand || slot == ItemSlots.OffHand)
			{
				foreach (IItem prop in Module.Props)
				{
					this.Selector.Items.Add(prop);
				}
			}

			foreach (IItem item in gameData.Items.All)
			{
				this.Selector.Items.Add(item);
			}

			this.ClassFilter = Classes.All;
		}

		public event DrawerEvent Close;
		public event DrawerEvent SelectionChanged;

		private enum Mode
		{
			All,
			Items,
			Props,
			Special,
		}

		public IItem Value
		{
			get
			{
				return (IItem)this.Selector.Value;
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

		public Classes ClassFilter
		{
			get
			{
				return this.classFilter;
			}
			set
			{
				this.classFilter = value;
				this.ClassExpander.Header = "Jobs: " + value.Describe(true);
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

		private bool OnFilter(object obj, string[] search = null)
		{
			if (obj is IItem item)
			{
				// skip items without names
				if (string.IsNullOrEmpty(item.Name))
					return false;

				if (this.mode == Mode.Items && (obj is Prop || item.Key == 0))
					return false;

				if (this.mode == Mode.Special && (obj is Prop || item.Key != 0))
					return false;

				if (this.mode == Mode.Props && !(obj is Prop))
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

				return matches;
			}

			return false;
		}

		private bool HasClass(Classes a, Classes b)
		{
			foreach (Classes job in Enum.GetValues(typeof(Classes)))
			{
				if (job == Classes.None)
					continue;

				if (a.HasFlag(job) && b.HasFlag(job))
				{
					return true;
				}
			}

			return false;
		}

		private void OnAllMode(object sender, RoutedEventArgs e)
		{
			this.mode = Mode.All;
			this.Selector?.FilterItems();
		}

		private void OnPropsMode(object sender, RoutedEventArgs e)
		{
			this.mode = Mode.Props;
			this.Selector.FilterItems();
		}

		private void OnItemsMode(object sender, RoutedEventArgs e)
		{
			this.mode = Mode.Items;
			this.Selector.FilterItems();
		}

		private void OnSpecialMode(object sender, RoutedEventArgs e)
		{
			this.mode = Mode.Special;
			this.Selector.FilterItems();
		}
	}
}
