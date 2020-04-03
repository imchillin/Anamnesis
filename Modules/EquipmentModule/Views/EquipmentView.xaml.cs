// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for EquipmentView.xaml.
	/// </summary>
	public partial class EquipmentView : UserControl
	{
		public EquipmentView()
		{
			this.InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			IViewService viewService = Module.Services.Get<IViewService>();
			EquipmentSelector selector = new EquipmentSelector();
			viewService.ShowDrawer(selector, "Select Equipment");
		}
	}
}
