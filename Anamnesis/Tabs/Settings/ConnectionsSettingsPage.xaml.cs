// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs.Settings;

using Anamnesis.Services;
using System.Collections.Generic;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for ConnectionsSettingsPage.xaml.
/// </summary>
public partial class ConnectionsSettingsPage : UserControl, ISettingSection
{
	public ConnectionsSettingsPage()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		// Initialize setting categories
		this.SettingCategories = new()
		{
			{ "ExternalServices", new SettingCategory("ExternalServices", this.ExternalServicesGroupBox) },
		};

		// Set up external services category settings
		this.SettingCategories["ExternalServices"].Settings.Add(new Setting("Settings_UseExternalRefreshBrio", this.Connections_External_UseExternalRefreshBrio));
		this.SettingCategories["ExternalServices"].Settings.Add(new Setting("Settings_UseExternalRefresh", this.Connections_External_UseExternalRefresh));
		this.SettingCategories["ExternalServices"].Settings.Add(new Setting("Settings_EnableNpcHack", this.Connections_External_EnableNpcHack));
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public static int LabelColumnWidth => 150;
	public Dictionary<string, SettingCategory> SettingCategories { get; }
}
