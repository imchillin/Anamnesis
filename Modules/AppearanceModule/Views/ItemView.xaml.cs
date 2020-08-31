// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Views
{
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using Anamnesis.AppearanceModule.ViewModels;
	using Anamnesis.GameData;
	using Anamnesis.WpfStyles.DependencyProperties;
	using Anamnesis.WpfStyles.Drawers;
	using PropertyChanged;

	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for ItemView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ItemView : UserControl
	{
		public static readonly IBind<ItemSlots> SlotDp = Binder.Register<ItemSlots, ItemView>("Slot");

		public ItemView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.ContentArea.DataContext = this;
		}

		public ItemSlots Slot
		{
			get => SlotDp.Get(this);
			set => SlotDp.Set(this, value);
		}

		public EquipmentBaseViewModel ViewModel
		{
			get;
			set;
		}

		public string SlotName
		{
			get
			{
				return this.Slot.ToDisplayName();
			}
		}

		public bool IsWeapon
		{
			get
			{
				return this.Slot == ItemSlots.MainHand || this.Slot == ItemSlots.OffHand;
			}
		}

		public ImageSource IconSource { get; set; }

		public bool CanDye
		{
			get
			{
				return this.Slot != ItemSlots.Ears
					&& this.Slot != ItemSlots.Neck
					&& this.Slot != ItemSlots.Wrists
					&& this.Slot != ItemSlots.LeftRing
					&& this.Slot != ItemSlots.RightRing;
			}
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
			EquipmentSelector selector = new EquipmentSelector(this.ViewModel.Slot);
			SelectorDrawer.Show<IItem>("Select " + this.SlotName, selector, this.ViewModel.Item, (v) => { this.ViewModel.Item = v; });
		}

		private void OnDyeClick(object sender, RoutedEventArgs e)
		{
			SelectorDrawer.Show<DyeSelector, IDye>("Select Dye", this.ViewModel.Dye, (v) => { this.ViewModel.Dye = v; });
		}

		private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.ViewModel = this.DataContext as EquipmentBaseViewModel;

			if (this.ViewModel == null)
				return;

			this.IconSource = this.ViewModel.Slot.GetIcon();
		}
	}
}
