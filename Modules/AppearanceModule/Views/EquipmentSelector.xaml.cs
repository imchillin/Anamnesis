// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Windows.Controls;
	using ConceptMatrix;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.GameData;
	using ConceptMatrix.WpfStyles.Drawers;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	public partial class EquipmentSelector : UserControl, SelectorDrawer.ISelectorView
	{
		private readonly ItemSlots slot;

		public EquipmentSelector(ItemSlots slot)
		{
			this.slot = slot;

			this.InitializeComponent();
			this.DataContext = this;

			IGameDataService gameData = Services.Get<IGameDataService>();
			this.Selector.Items.Add(EquipmentBaseViewModel.NoneItem);
			this.Selector.Items.Add(EquipmentBaseViewModel.NpcbodyItem);
			foreach (IItem item in gameData.Items.All)
			{
				this.Selector.Items.Add(item);
			}

			this.Selector.FilterItems();
		}

		public event DrawerEvent Close;
		public event DrawerEvent SelectionChanged;

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

				if (!SearchUtility.Matches(item.Name, search))
					return false;

				return true;
			}

			return false;
		}
	}
}
