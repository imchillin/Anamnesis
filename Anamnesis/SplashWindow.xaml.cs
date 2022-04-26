// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System.ComponentModel;
using System.Windows.Input;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class SplashWindow
{
	private static SplashWindow? instance;

	public SplashWindow()
	{
		this.InitializeComponent();
		instance = this;
	}

	public static void HideWindow()
	{
		instance?.Hide();
	}

	public static void ShowWindow()
	{
		instance?.Show();
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		instance = null;
	}

	private void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			this.DragMove();
		}
	}
}
