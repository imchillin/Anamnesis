// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Panels;
using XivToolsWpf.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : ChromedWindow
{
	public MainWindow()
	{
		this.InitializeComponent();

		// if OverlayMode...
		{
			IPanelGroupHost wnd = new OverlayWindow();
			NavigationPanel nav = new(wnd);
			wnd.PanelGroupArea.Content = nav;
			wnd.Show();

			this.Close();
		}
		//// else...
	}
}
