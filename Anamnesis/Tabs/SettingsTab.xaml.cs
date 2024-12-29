// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Core;
using Anamnesis.Services;
using Anamnesis.Tabs.Settings;
using FontAwesome.Sharp;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

/// <summary>
/// Interaction logic for SettingsTab.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class SettingsTab : System.Windows.Controls.UserControl
{
	public SettingsTab()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		// Set up settings pages
		this.Pages.Add(new Page<GeneralSettingsPage>(IconChar.Cog, "SettingsPages", "General"));
		this.Pages.Add(new Page<InputSettingsPage>(IconChar.Keyboard, "SettingsPages", "Input"));
		this.Pages.Add(new Page<PersonalizationSettingsPage>(IconChar.Palette, "SettingsPages", "Personalization"));
		this.Pages.Add(new Page<ConnectionsSettingsPage>(IconChar.NetworkWired, "SettingsPages", "Connections"));

		// Set the first page as active
		this.SelectedPage = this.Pages[0];
		this.Pages[0].IsActive = true;
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public ObservableCollection<Page> Pages { get; private set; } = new();
	public Page SelectedPage { get; set; }

	private void SelectPage(object sender, MouseButtonEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return;

		if (senderElement.DataContext is not Page selectedPage)
			return;

		this.SelectedPage = selectedPage;
		foreach (var page in this.Pages)
		{
			page.IsActive = senderElement.DataContext == page;
		}
	}
}
