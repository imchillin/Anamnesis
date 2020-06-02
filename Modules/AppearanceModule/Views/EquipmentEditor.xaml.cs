// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Files;
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

		public IBaseMemoryOffset BaseOffset
		{
			get;
			private set;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnActorChanged(this.DataContext as Actor);
		}

		[SuppressPropertyChangedWarnings]
		private void OnActorChanged(Actor actor)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = false;
			});

			this.eqMem?.Dispose();
			this.MainHand?.Dispose();
			this.OffHand?.Dispose();
			this.eqMem?.Dispose();
			this.Head?.Dispose();
			this.Body?.Dispose();
			this.Hands?.Dispose();
			this.Legs?.Dispose();
			this.Feet?.Dispose();
			this.Ears?.Dispose();
			this.Neck?.Dispose();
			this.Wrists?.Dispose();
			this.LeftRing?.Dispose();
			this.RightRing?.Dispose();

			this.MainHand = null;
			this.OffHand = null;
			this.Head = null;
			this.Body = null;
			this.Hands = null;
			this.Legs = null;
			this.Feet = null;
			this.Ears = null;
			this.Neck = null;
			this.Wrists = null;
			this.LeftRing = null;
			this.RightRing = null;

			if (actor == null || (actor.Type != ActorTypes.Player && actor.Type != ActorTypes.BattleNpc && actor.Type != ActorTypes.EventNpc))
				return;

			this.BaseOffset = actor.BaseAddress;

			// Weapon slots
			this.MainHand = new EquipmentWeaponViewModel(ItemSlots.MainHand, this.BaseOffset);
			this.OffHand = new EquipmentWeaponViewModel(ItemSlots.OffHand, this.BaseOffset);

			// Equipment slots
			this.eqMem = actor.BaseAddress.GetMemory(Offsets.Main.ActorEquipment);
			this.eqMem.Name = "Equipment";

			this.Head = new EquipmentItemViewModel(this.eqMem, ItemSlots.Head, this.BaseOffset);
			this.Body = new EquipmentItemViewModel(this.eqMem, ItemSlots.Body, this.BaseOffset);
			this.Hands = new EquipmentItemViewModel(this.eqMem, ItemSlots.Hands, this.BaseOffset);
			this.Legs = new EquipmentItemViewModel(this.eqMem, ItemSlots.Legs, this.BaseOffset);
			this.Feet = new EquipmentItemViewModel(this.eqMem, ItemSlots.Feet, this.BaseOffset);
			this.Ears = new EquipmentItemViewModel(this.eqMem, ItemSlots.Ears, this.BaseOffset);
			this.Neck = new EquipmentItemViewModel(this.eqMem, ItemSlots.Neck, this.BaseOffset);
			this.Wrists = new EquipmentItemViewModel(this.eqMem, ItemSlots.Wrists, this.BaseOffset);
			this.LeftRing = new EquipmentItemViewModel(this.eqMem, ItemSlots.LeftRing, this.BaseOffset);
			this.RightRing = new EquipmentItemViewModel(this.eqMem, ItemSlots.RightRing, this.BaseOffset);

			Application.Current.Dispatcher.Invoke(() =>
			{
				////this.ContentArea.DataContext = null;
				this.ContentArea.DataContext = this;
				this.IsEnabled = true;
			});
		}
	}
}
