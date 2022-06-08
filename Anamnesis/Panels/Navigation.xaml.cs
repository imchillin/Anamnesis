// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using System.Windows.Input;

/// <summary>
/// Interaction logic for Navigation.xaml.
/// </summary>
public partial class Navigation : PanelBase
{
	public Navigation()
	{
		this.InitializeComponent();
	}

	private void OnIconMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.DragMove();
	}
}
