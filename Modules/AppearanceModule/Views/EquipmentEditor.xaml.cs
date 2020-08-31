// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule.Views
{
	using System.Windows.Controls;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for EquipmentView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class EquipmentEditor : UserControl
	{
		public EquipmentEditor()
		{
			this.InitializeComponent();
		}
	}
}
