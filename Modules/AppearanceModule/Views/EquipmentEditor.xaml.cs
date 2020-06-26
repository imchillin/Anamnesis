// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.GameData;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for EquipmentView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class EquipmentEditor : UserControl
	{
		private IMemory<Equipment> eqMem;

		public EquipmentEditor()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
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

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Actor actor = this.DataContext as Actor;

			if (actor == null)
				return;

			// Weapon slots
			this.MainHand = new EquipmentWeaponViewModel(ItemSlots.MainHand, actor);
			this.OffHand = new EquipmentWeaponViewModel(ItemSlots.OffHand, actor);

			// Equipment slots
			this.eqMem = actor.GetMemory(Offsets.Main.ActorEquipment);

			this.Head = new EquipmentItemViewModel(this.eqMem, ItemSlots.Head, actor);
			this.Body = new EquipmentItemViewModel(this.eqMem, ItemSlots.Body, actor);
			this.Hands = new EquipmentItemViewModel(this.eqMem, ItemSlots.Hands, actor);
			this.Legs = new EquipmentItemViewModel(this.eqMem, ItemSlots.Legs, actor);
			this.Feet = new EquipmentItemViewModel(this.eqMem, ItemSlots.Feet, actor);
			this.Ears = new EquipmentItemViewModel(this.eqMem, ItemSlots.Ears, actor);
			this.Neck = new EquipmentItemViewModel(this.eqMem, ItemSlots.Neck, actor);
			this.Wrists = new EquipmentItemViewModel(this.eqMem, ItemSlots.Wrists, actor);
			this.LeftRing = new EquipmentItemViewModel(this.eqMem, ItemSlots.LeftRing, actor);
			this.RightRing = new EquipmentItemViewModel(this.eqMem, ItemSlots.RightRing, actor);
		}
	}
}
