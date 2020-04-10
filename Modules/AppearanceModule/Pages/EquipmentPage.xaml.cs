// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Pages
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Files;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for EquipmentView.xaml.
	/// </summary>
	public partial class EquipmentPage : UserControl
	{
		private IMemory<Equipment> eqMem;

		public EquipmentPage()
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
			App.Current.Dispatcher.Invoke(() =>
			{
				this.IsEnabled = selection != null;
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

			if (selection == null)
				return;

			// Weapon slots
			this.MainHand = new EquipmentWeaponViewModel(ItemSlots.MainHand, selection);
			this.OffHand = new EquipmentWeaponViewModel(ItemSlots.OffHand, selection);

			// Equipment slots
			this.eqMem = selection.BaseAddress.GetMemory(Offsets.ActorEquipment);
			this.Head = new EquipmentItemViewModel(this.eqMem, ItemSlots.Head, selection);
			this.Body = new EquipmentItemViewModel(this.eqMem, ItemSlots.Body, selection);
			this.Hands = new EquipmentItemViewModel(this.eqMem, ItemSlots.Hands, selection);
			this.Legs = new EquipmentItemViewModel(this.eqMem, ItemSlots.Legs, selection);
			this.Feet = new EquipmentItemViewModel(this.eqMem, ItemSlots.Feet, selection);
			this.Ears = new EquipmentItemViewModel(this.eqMem, ItemSlots.Ears, selection);
			this.Neck = new EquipmentItemViewModel(this.eqMem, ItemSlots.Neck, selection);
			this.Wrists = new EquipmentItemViewModel(this.eqMem, ItemSlots.Wrists, selection);
			this.LeftRing = new EquipmentItemViewModel(this.eqMem, ItemSlots.LeftRing, selection);
			this.RightRing = new EquipmentItemViewModel(this.eqMem, ItemSlots.RightRing, selection);

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.ContentArea.DataContext = null;
				this.ContentArea.DataContext = this;
			});
		}

		private async void OnOpenClicked(object sender, RoutedEventArgs e)
		{
			IFileService fileService = Module.Services.Get<IFileService>();
			FileBase file = await fileService.OpenAny(EquipmentSetFile.FileType, LegacyEquipmentSetFile.FileType);

			if (file is LegacyEquipmentSetFile legacyFile)
				file = legacyFile.Upgrade();

			if (file is EquipmentSetFile eqFile)
			{
				eqFile.MainHand.Write(this.MainHand);
				eqFile.OffHand.Write(this.OffHand);
				eqFile.Head.Write(this.Head);
				eqFile.Body.Write(this.Body);
				eqFile.Hands.Write(this.Hands);
				eqFile.Legs.Write(this.Legs);
				eqFile.Feet.Write(this.Feet);
				eqFile.Ears.Write(this.Ears);
				eqFile.Neck.Write(this.Neck);
				eqFile.Wrists.Write(this.Wrists);
				eqFile.LeftRing.Write(this.LeftRing);
				eqFile.RightRing.Write(this.RightRing);
			}
		}

		private void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			IFileService fileService = Module.Services.Get<IFileService>();
			EquipmentSetFile eqFile = new EquipmentSetFile();
			eqFile.MainHand.Read(this.MainHand);
			eqFile.OffHand.Read(this.OffHand);
			eqFile.Head.Read(this.Head);
			eqFile.Body.Read(this.Body);
			eqFile.Hands.Read(this.Hands);
			eqFile.Legs.Read(this.Legs);
			eqFile.Feet.Read(this.Feet);
			eqFile.Ears.Read(this.Ears);
			eqFile.Neck.Read(this.Neck);
			eqFile.Wrists.Read(this.Wrists);
			eqFile.LeftRing.Read(this.LeftRing);
			eqFile.RightRing.Read(this.RightRing);

			fileService.Save(eqFile);
		}
	}
}
