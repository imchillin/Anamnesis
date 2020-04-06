// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for EquipmentView.xaml.
	/// </summary>
	public partial class EquipmentView : UserControl
	{
		private IMemory<Equipment> eqMem;

		public EquipmentView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			ISelectionService selection = Module.Services.Get<ISelectionService>();
			selection.SelectionChanged += this.OnSelectionChanged;
			this.OnSelectionChanged(selection.CurrentSelection);
		}

		public EquipmentWeaponViewModel MainHand { get; set; }
		public EquipmentWeaponViewModel OffHand { get; set; }
		public EquipmentItemViewModel Head { get; set; }
		public EquipmentItemViewModel Body { get; set; }
		public EquipmentItemViewModel Hands { get; set; }
		public EquipmentItemViewModel Legs { get; set; }
		public EquipmentItemViewModel Feet { get; set; }
		public EquipmentItemViewModel Ears { get; set; }
		public EquipmentItemViewModel Neck { get; set; }
		public EquipmentItemViewModel Wrists { get; set; }
		public EquipmentItemViewModel LeftRing { get; set; }
		public EquipmentItemViewModel RightRing { get; set; }

		private void OnSelectionChanged(Selection selection)
		{
			if (this.eqMem != null)
				this.eqMem.Dispose();

			if (selection == null)
				return;

			this.eqMem = selection.BaseAddress.GetMemory(Offsets.ActorEquipment);
			Equipment eq = this.eqMem.Value;

			IMemory<Weapon> mhMem = selection.BaseAddress.GetMemory(Offsets.MainHand);
			IMemory<Weapon> ohMem = selection.BaseAddress.GetMemory(Offsets.OffHand);

			// Weapon slots
			this.MainHand = new EquipmentWeaponViewModel(mhMem, ItemSlots.MainHand);
			this.OffHand = new EquipmentWeaponViewModel(ohMem, ItemSlots.OffHand);

			// Equipment slots
			this.Head = new EquipmentItemViewModel(eq, ItemSlots.Head);
			this.Body = new EquipmentItemViewModel(eq, ItemSlots.Body);
			this.Hands = new EquipmentItemViewModel(eq, ItemSlots.Hands);
			this.Legs = new EquipmentItemViewModel(eq, ItemSlots.Legs);
			this.Feet = new EquipmentItemViewModel(eq, ItemSlots.Feet);
			this.Ears = new EquipmentItemViewModel(eq, ItemSlots.Ears);
			this.Neck = new EquipmentItemViewModel(eq, ItemSlots.Neck);
			this.Wrists = new EquipmentItemViewModel(eq, ItemSlots.Wrists);
			this.LeftRing = new EquipmentItemViewModel(eq, ItemSlots.LeftRing);
			this.RightRing = new EquipmentItemViewModel(eq, ItemSlots.RightRing);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.ContentArea.DataContext = null;
				this.ContentArea.DataContext = this;
			});
		}
	}
}
