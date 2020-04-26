// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.ComponentModel;
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.GameData;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using PropertyChanged;

	using Vector = ConceptMatrix.Vector;

	/// <summary>
	/// Interaction logic for ItemView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ItemView : UserControl
	{
		public static readonly IBind<ItemSlots> SlotDp = Binder.Register<ItemSlots, ItemView>("Slot");

		private IGameDataService gameData;

		public ItemView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.ContentArea.DataContext = this;
			this.gameData = Services.Get<IGameDataService>();
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

		private async void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();

			EquipmentSelector selector = new EquipmentSelector(this.ViewModel.Slot);
			selector.Value = this.ViewModel.Item;
			await viewService.ShowDrawer(selector, "Select " + this.SlotName);

			if (selector.Value == null)
				return;

			this.ViewModel.Item = selector.Value;
		}

		private async void OnDyeClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Services.Get<IViewService>();

			DyeSelector selector = new DyeSelector();
			selector.Value = this.ViewModel.Dye;
			await viewService.ShowDrawer(selector, "Select Dye");

			if (selector.Value == null)
				return;

			this.ViewModel.Dye = selector.Value;
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

			this.SlotIcon.Source = this.ViewModel.Slot.GetIcon();
		}

		private void OnZeroScaleClick(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Scale = Vector.Zero;
		}

		private void OnOneScaleClick(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Scale = Vector.One;
		}
	}
}
