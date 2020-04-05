// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media.Media3D;
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

			this.DataContext = this;

			////this.MainHandEq.Value = gameData.Items.Get(29412);
		}

		public ItemViewModel MainHand { get; set; } = new ItemViewModel(ItemSlots.MainHand);
		public ItemViewModel OffHand { get; set; } = new ItemViewModel(ItemSlots.OffHand);
		public ItemViewModel Head { get; set; } = new ItemViewModel(ItemSlots.Head);
		public ItemViewModel Body { get; set; } = new ItemViewModel(ItemSlots.Body);
		public ItemViewModel Waist { get; set; } = new ItemViewModel(ItemSlots.Waist);
		public ItemViewModel Hands { get; set; } = new ItemViewModel(ItemSlots.Hands);
		public ItemViewModel Legs { get; set; } = new ItemViewModel(ItemSlots.Legs);
		public ItemViewModel Feet { get; set; } = new ItemViewModel(ItemSlots.Feet);
		public ItemViewModel Ears { get; set; } = new ItemViewModel(ItemSlots.Ears);
		public ItemViewModel Neck { get; set; } = new ItemViewModel(ItemSlots.Neck);
		public ItemViewModel Wrists { get; set; } = new ItemViewModel(ItemSlots.Wrists);
		public ItemViewModel LeftRing { get; set; } = new ItemViewModel(ItemSlots.LeftRing);
		public ItemViewModel RightRing { get; set; } = new ItemViewModel(ItemSlots.RightRing);
	}
}
