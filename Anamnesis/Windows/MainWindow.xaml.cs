// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using XivToolsWpf.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : ChromedWindow
{
	public MainWindow()
	{
		this.InitializeComponent();

		OverlayWindow wnd = new();
		wnd.Show();

		this.Close();
	}
}
