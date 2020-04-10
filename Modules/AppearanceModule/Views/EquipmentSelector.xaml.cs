// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Windows.Controls;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for EquipmentSelector.xaml.
	/// </summary>
	public partial class EquipmentSelector : UserControl, IDrawer
	{
		private ItemSlots slot;

		public EquipmentSelector(ItemSlots slot)
		{
			this.slot = slot;

			this.InitializeComponent();
			this.DataContext = this;

			IGameDataService gameData = Module.Services.Get<IGameDataService>();
			this.Selector.Items.Add(EquipmentBaseViewModel.NoneItem);
			foreach (IItem item in gameData.Items.All)
			{
				this.Selector.Items.Add(item);
			}

			this.Selector.FilterItems();
		}

		public event DrawerEvent Close;

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

		private void OnClose()
		{
			this.Close?.Invoke();
		}

		private bool OnFilter(object obj, string[] search = null)
		{
			if (obj is IItem item)
			{
				// skip items without names
				if (string.IsNullOrEmpty(item.Name))
					return false;

				if (!item.FitsInSlot(this.slot))
					return false;

				if (!SearchUtility.Matches(item.Name, search))
					return false;

				return true;
			}

			return false;
		}
	}
}
