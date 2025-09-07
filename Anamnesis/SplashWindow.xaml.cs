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
	private static SplashWindow? s_instance;

	public SplashWindow()
	{
		this.InitializeComponent();
		s_instance = this;
	}

	public static void HideWindow()
	{
		s_instance?.Hide();
	}

	public static void ShowWindow()
	{
		s_instance?.Show();
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		s_instance = null;
	}

	private void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			this.DragMove();
		}
	}
}
