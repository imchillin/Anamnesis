// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for PanelGroup.xaml.
/// </summary>
public partial class PanelGroup : UserControl
{
	private readonly IPanelGroupHost host;

	public PanelGroup(IPanelGroupHost host)
	{
		this.host = host;
		this.InitializeComponent();
	}

	private void OnTitlebarMousDown(object sender, MouseButtonEventArgs e)
	{
		this.host.DragMove();
	}
}
