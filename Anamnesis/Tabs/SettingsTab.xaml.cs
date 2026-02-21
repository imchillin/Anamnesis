// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Core;
using Anamnesis.Services;
using Anamnesis.Tabs.Settings;
using FontAwesome.Sharp;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

public interface ISettingSection
{
	Dictionary<string, SettingCategory> SettingCategories { get; }
}

/// <summary>
/// Interaction logic for SettingsTab.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class SettingsTab : System.Windows.Controls.UserControl
{
	private string searchQuery = string.Empty;

	public SettingsTab()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		// Set up settings pages
		this.Pages = new ObservableCollection<SettingsPage>
		{
			new SettingsPage<GeneralSettingsPage>(IconChar.Cog, "SettingsPages", "General"),
			new SettingsPage<InputSettingsPage>(IconChar.Keyboard, "SettingsPages", "Input"),
			new SettingsPage<PersonalizationSettingsPage>(IconChar.Palette, "SettingsPages", "Personalization"),
			new SettingsPage<ConnectionsSettingsPage>(IconChar.NetworkWired, "SettingsPages", "Connections"),
			new SettingsPage<BackupAndRecoverySettingsPage>(IconChar.Database, "SettingsPages", "BackupAndRecovery"),
		};

		// Force initialization of all pages
		// This is done to ensure that all settings are configured before the user interacts with them
		foreach (var page in this.Pages)
		{
			page.IsActive = true;
			var content = page.Content;
			page.IsActive = false;
		}

		// Set the first page as active
		this.SelectedPage = this.Pages[0];
		this.Pages[0].IsActive = true;
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public ObservableCollection<SettingsPage> Pages { get; private set; }

	[DependsOn(nameof(Pages))]
	public SettingsPage SelectedPage { get; set; }

	public string SearchQuery
	{
		get => this.searchQuery;
		set
		{
			this.searchQuery = value;
			this.FilterSettings();
		}
	}

	public void OnSearchClearClicked(object sender, RoutedEventArgs e)
	{
		this.SearchQuery = string.Empty;
	}

	private void SelectPage(object sender, MouseButtonEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return;

		if (senderElement.DataContext is not SettingsPage selectedPage)
			return;

		this.SelectedPage = selectedPage;
		foreach (var page in this.Pages)
		{
			page.IsActive = page == selectedPage;
		}
	}

	private void FilterSettings()
	{
		// Disable the settings sidebar if the search query is not empty
		this.SettingsSidebar.IsEnabled = this.SearchQuery.Length == 0;

		foreach (var page in this.Pages)
		{
			page.FilterContent(this.SearchQuery);
			page.IsActive = string.IsNullOrEmpty(this.SearchQuery) ? page == this.SelectedPage : page.HasVisibleSettings;
		}
	}

	[AddINotifyPropertyChangedInterface]
	public abstract class SettingsPage(IconChar icon, string context, string name) : Page(icon, context, name)
	{
		public abstract Dictionary<string, SettingCategory> SettingCategories { get; }
		public bool HasVisibleSettings => this.SettingCategories.Any(category => category.Value.Element.Visibility == Visibility.Visible);

		public abstract void FilterContent(string query);
	}

	public class SettingsPage<T>(IconChar icon, string context, string name) : SettingsPage(icon, context, name)
		where T : System.Windows.Controls.UserControl, ISettingSection
	{
		private T? content;

		public override Dictionary<string, SettingCategory> SettingCategories => this.content?.SettingCategories ?? new Dictionary<string, SettingCategory>();

		public override void FilterContent(string query)
		{
			if (this.Content is not ISettingSection filterableContent)
				return;

			foreach (var category in this.SettingCategories)
			{
				category.Value.UpdateVisibility(query);
			}
		}

		protected override System.Windows.Controls.UserControl CreateContent()
		{
			this.content ??= Activator.CreateInstance<T>();
			Debug.Assert(this.content != null, $"Failed to create page content: {typeof(T)}");
			return this.content;
		}
	}
}

public class Setting(string key, UIElement element)
{
	public string Key { get; } = key;
	public string Text { get; } = LocalizationService.GetString(key) ?? key;
	public UIElement Element { get; } = element;
}

public class SettingCategory(string key, UIElement element)
{
	public string Key { get; } = key;
	public UIElement Element { get; } = element;
	public List<Setting> Settings { get; set; } = new List<Setting>();

	public void UpdateVisibility(string query)
	{
		// If the query is empty, reset visibility of all settings
		if (query.Length == 0)
		{
			this.Element.Visibility = Visibility.Visible;
			this.Settings.ForEach(setting => setting.Element.Visibility = Visibility.Visible);
			return;
		}

		// Check if the category header or any setting label matches the query
		bool categoryMatches = this.Key.Contains(query, StringComparison.OrdinalIgnoreCase);
		bool anyVisible = this.Settings.Any(setting => categoryMatches || setting.Text.Contains(query, StringComparison.OrdinalIgnoreCase));

		this.Element.Visibility = anyVisible ? Visibility.Visible : Visibility.Collapsed;
		this.Settings.ForEach(setting => setting.Element.Visibility = categoryMatches || setting.Text.Contains(query, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed);
	}
}
