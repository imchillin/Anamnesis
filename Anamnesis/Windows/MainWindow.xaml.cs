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
			OverlayWindow wnd = new();
			Navigation nav = new();
			nav.Host = wnd;
			wnd.ContentArea.Content = nav;
			wnd.Show();

			this.Close();
		}
		//// else...
	}
}
