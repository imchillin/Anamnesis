// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Keyboard;
using Anamnesis.Services;
using Anamnesis.Views;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for ActorTab.xaml.
/// </summary>
public partial class ActorTab : UserControl
{
	public ActorTab()
	{
		this.InitializeComponent();

		HotkeyService.RegisterHotkeyHandler("MainWindow.SceneTab", () => this.SceneTab.Focus());
		HotkeyService.RegisterHotkeyHandler("MainWindow.AppearanceTab", () => this.AppearanceTab.Focus());
		HotkeyService.RegisterHotkeyHandler("MainWindow.PoseTab", () => this.PoseTab.Focus());
		HotkeyService.RegisterHotkeyHandler("MainWindow.ActionTab", () => this.ActionTab.Focus());
	}

	private void OnHistoryClick(object sender, RoutedEventArgs e)
	{
		ViewService.ShowDrawer<HistoryView>();
	}
}
