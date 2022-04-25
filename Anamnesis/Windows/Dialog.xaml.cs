// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Windows;

using System;
using System.Windows;
using System.Windows.Input;
using Anamnesis.Services;

/// <summary>
/// Interaction logic for Dialog.xaml.
/// </summary>
public partial class Dialog : Window
{
	public Dialog()
	{
		this.InitializeComponent();

		this.Owner = App.Current.MainWindow;
	}

	private void OnTitleBarMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			this.DragMove();
		}
	}

	private void Window_Activated(object sender, EventArgs e)
	{
		this.ActiveBorder.Visibility = Visibility.Visible;
		this.InActiveBorder.Visibility = Visibility.Collapsed;
	}

	private void Window_Deactivated(object sender, EventArgs e)
	{
		this.ActiveBorder.Visibility = Visibility.Collapsed;
		this.InActiveBorder.Visibility = Visibility.Visible;
	}

	private void OnCloseClick(object sender, RoutedEventArgs e)
	{
		if (this.ContentArea.Content is IDialog dlg)
		{
			dlg.Cancel();
			return;
		}

		this.Close();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		SplashWindow.HideWindow();
	}

	private void Window_Unloaded(object sender, RoutedEventArgs e)
	{
		SplashWindow.ShowWindow();
	}
}
