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

			IGameDataService gameData = Module.Services.Get<IGameDataService>();

			this.MainHandEq.Value = gameData.Items.Get(29412);
		}
	}
}
