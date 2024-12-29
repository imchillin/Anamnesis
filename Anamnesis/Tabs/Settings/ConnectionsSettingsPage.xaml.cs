// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs.Settings;

using Anamnesis.Services;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for ConnectionsSettingsPage.xaml.
/// </summary>
public partial class ConnectionsSettingsPage : UserControl
{
	public ConnectionsSettingsPage()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public static SettingsService SettingsService => SettingsService.Instance;
}
