// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.ComponentModel;
	using System.Text.RegularExpressions;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Services;
	using ConceptMatrix.WpfStyles.Drawers;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ItemView.xaml.
	/// </summary>
	public partial class ItemView : UserControl, INotifyPropertyChanged
	{
		private IGameDataService gameData;

		public ItemView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.gameData = Module.Services.Get<IGameDataService>();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public EquipmentBaseViewModel ViewModel
		{
			get
			{
				return this.DataContext as EquipmentBaseViewModel;
			}
		}

		private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is ItemView view)
			{
				view.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(e.Property.Name));
			}
		}

		private async void OnClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();

			EquipmentSelector selector = new EquipmentSelector(this.ViewModel.Slot);
			selector.Value = this.ViewModel.Item;
			await viewService.ShowDrawer(selector, "Select " + this.ViewModel.SlotName);
			this.ViewModel.Item = selector.Value;
		}

		private async void OnDyeClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();

			DyeSelector selector = new DyeSelector();
			selector.Value = this.ViewModel.Dye;
			await viewService.ShowDrawer(selector, "Select Dye");
			this.ViewModel.Dye = selector.Value;
		}

		private async void OnColorClick(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();

			ColorSelectorDrawer selector = new ColorSelectorDrawer();
			selector.Value = this.ViewModel.Color;
			await viewService.ShowDrawer(selector, "Color");
			this.ViewModel.Color = selector.Value;
		}

		private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.ViewModel == null)
				return;

			this.SlotIcon.Source = this.ViewModel.Slot.GetIcon();
		}

		private void OnZeroScaleClick(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Scale = new Vector3D(0, 0, 0);
		}
	}
}
