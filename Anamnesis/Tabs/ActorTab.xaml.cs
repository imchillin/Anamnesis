// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Actor.Pages;
using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Views;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for ActorTab.xaml.
/// </summary>
public partial class ActorTab : UserControl
{
	private bool loading = true;

	public ActorTab()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		TargetService.ActorSelected += this.OnActorSelected;
		this.Tabs.CollectionChanged += this.OnTabsChanged;

		this.AddPage<CharacterPage>("AppearanceTab", IconChar.UserEdit);
		this.AddPage<ActionPage>("ActionTab", IconChar.Biking);
		this.AddPage<PosePage>("PoseTab", IconChar.Running);

		this.SortTabs();

		this.Tabs[0].IsActive = true;
	}

	public ObservableCollection<Core.Page> Tabs { get; private set; } = new();
	public ObservableCollection<Core.Page> Pages { get; private set; } = new();

	private static HistoryContext GetHistoryContextForTab(Page? page)
	{
		return page?.Name switch
		{
			"AppearanceTab" => HistoryContext.Appearance,
			"PoseTab" => HistoryContext.Posing,
			_ => HistoryContext.Other,
		};
	}

	private void AddPage<T>(string name, IconChar icon)
		where T : UserControl
	{
		Page<T> page = new Page<T>(icon, "ActorTabs", name);
		this.Tabs.Add(page);
		this.Pages.Add(page);
	}

	private Core.Page GetTab(string name)
	{
		foreach (Core.Page tab in this.Tabs)
		{
			if (tab.Name == name)
			{
				return tab;
			}
		}

		throw new Exception($"No page found with name: {name}");
	}

	private void OnTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (this.loading)
			return;

		foreach (var tab in this.Tabs)
		{
			if (!SettingsService.Current.ActorTabOrder.ContainsKey(tab.Name))
				SettingsService.Current.ActorTabOrder.Add(tab.Name, 0);

			SettingsService.Current.ActorTabOrder[tab.Name] = this.Tabs.IndexOf(tab);
		}

		SettingsService.Save();
	}

	private void SortTabs()
	{
		this.loading = true;

		List<Core.Page> pages = new();
		foreach ((string name, int index) in SettingsService.Current.ActorTabOrder)
		{
			var tab = this.GetTab(name);
			tab.Index = index;
			pages.Add(tab);
		}

		pages.Sort((a, b) => a.Index.CompareTo(b.Index));

		// Add any missing pages
		foreach (var page in this.Pages)
		{
			if (!pages.Contains(page))
			{
				pages.Add(page);
			}
		}

		this.Tabs.Clear();
		foreach (var page in pages)
		{
			this.Tabs.Add(page);
		}

		this.loading = false;
	}

	private void OnActorSelected(ActorMemory? actor)
	{
		foreach (var page in this.Pages)
		{
			page.DataContext = actor;
		}
	}

	private void OnTabSelected(object sender, RoutedEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return;

		foreach (var page in this.Pages)
		{
			page.IsActive = senderElement.DataContext == page;
		}

		// Set the history context based on the selected tab
		HistoryContext context = GetHistoryContextForTab(senderElement.DataContext as Page);
		HistoryService.SetContext(context);
	}

	private void OnHistoryClick(object sender, RoutedEventArgs e)
	{
		ViewService.ShowDrawer<HistoryView>();
	}
}
