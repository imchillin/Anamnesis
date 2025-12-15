// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using PropertyChanged;
using System.Windows.Controls;

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
