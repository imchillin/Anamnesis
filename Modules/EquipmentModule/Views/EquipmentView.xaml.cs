// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for EquipmentView.xaml.
	/// </summary>
	public partial class EquipmentView : UserControl
	{
		public EquipmentView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			ISelectionService selection = Module.Services.Get<ISelectionService>();
			selection.SelectionChanged += this.OnSelectionChanged;
			this.OnSelectionChanged(selection.CurrentSelection);

			////this.MainHandEq.Value = gameData.Items.Get(29412);
		}

		public ItemViewModel MainHand { get; set; }
		public ItemViewModel OffHand { get; set; }
		public ItemViewModel Head { get; set; }
		public ItemViewModel Body { get; set; }
		public ItemViewModel Hands { get; set; }
		public ItemViewModel Legs { get; set; }
		public ItemViewModel Feet { get; set; }
		public ItemViewModel Ears { get; set; }
		public ItemViewModel Neck { get; set; }
		public ItemViewModel Wrists { get; set; }
		public ItemViewModel LeftRing { get; set; }
		public ItemViewModel RightRing { get; set; }

		private void OnSelectionChanged(Selection selection)
		{
			IMemory<Equipment> eqMem = Offsets.ActorEquipment.GetMemory(selection.BaseAddress);
			Equipment eq = eqMem.Value;

			this.MainHand = new ItemViewModel(eq, ItemSlots.MainHand);
			this.OffHand = new ItemViewModel(eq, ItemSlots.OffHand);
			this.Head = new ItemViewModel(eq, ItemSlots.Head);
			this.Body = new ItemViewModel(eq, ItemSlots.Body);
			this.Hands = new ItemViewModel(eq, ItemSlots.Hands);
			this.Legs = new ItemViewModel(eq, ItemSlots.Legs);
			this.Feet = new ItemViewModel(eq, ItemSlots.Feet);
			this.Ears = new ItemViewModel(eq, ItemSlots.Ears);
			this.Neck = new ItemViewModel(eq, ItemSlots.Neck);
			this.Wrists = new ItemViewModel(eq, ItemSlots.Wrists);
			this.LeftRing = new ItemViewModel(eq, ItemSlots.LeftRing);
			this.RightRing = new ItemViewModel(eq, ItemSlots.RightRing);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.ContentArea.DataContext = null;
				this.ContentArea.DataContext = this;
			});
		}
	}
}
