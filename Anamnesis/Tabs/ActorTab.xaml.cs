// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Actor.Pages;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Views;
using FontAwesome.Sharp;
using PropertyChanged;
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

	public ObservableCollection<Page> Tabs { get; private set; } = new();
	public ObservableCollection<Page> Pages { get; private set; } = new();

	private void AddPage<T>(string name, IconChar icon)
		where T : UserControl
	{
		Page<T> page = new Page<T>(icon, name);
		this.Tabs.Add(page);
		this.Pages.Add(page);
	}

	private Page GetPage(string name)
	{
		foreach (Page page in this.Tabs)
		{
			if (page.Name == name)
			{
				return page;
			}
		}

		throw new Exception($"No page found with name: {name}");
	}

	private void OnTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (this.loading)
			return;

		foreach (Page page in this.Tabs)
		{
			if (!SettingsService.Current.ActorTabOrder.ContainsKey(page.Name))
				SettingsService.Current.ActorTabOrder.Add(page.Name, 0);

			SettingsService.Current.ActorTabOrder[page.Name] = this.Tabs.IndexOf(page);
		}

		SettingsService.Save();
	}

	private void SortTabs()
	{
		this.loading = true;

		List<Page> pages = new List<Page>();
		foreach ((string name, int index) in SettingsService.Current.ActorTabOrder)
		{
			Page page = this.GetPage(name);
			page.Index = index;
			pages.Add(page);
		}

		pages.Sort((a, b) => a.Index.CompareTo(b.Index));

		// Add any missing pages
		foreach (Page page in this.Pages)
		{
			if (!pages.Contains(page))
			{
				pages.Add(page);
			}
		}

		this.Tabs.Clear();
		foreach (Page page in pages)
		{
			this.Tabs.Add(page);
		}

		this.loading = false;
	}

	private void OnActorSelected(ActorMemory? actor)
	{
		foreach (Page page in this.Pages)
		{
			page.DataContext = actor;
		}
	}

	private void OnTabSelected(object sender, RoutedEventArgs e)
	{
		if (sender is not FrameworkElement senderElement)
			return;

		foreach (Page page in this.Pages)
		{
			page.IsActive = senderElement.DataContext == page;
		}
	}

	private void OnHistoryClick(object sender, RoutedEventArgs e)
	{
		ViewService.ShowDrawer<HistoryView>();
	}

	[AddINotifyPropertyChangedInterface]
	public abstract class Page
	{
		private UserControl? control;

		public Page(IconChar icon, string name)
		{
			this.Icon = icon;
			this.Name = name;
			this.DisplayNameKey = $"ActorTabs_{name}";
			this.TooltipKey = $"ActorTabs_{name}_Tooltip";

			HotkeyService.RegisterHotkeyHandler($"MainWindow.{name}", () => this.IsActive = true);
		}

		public string Name { get; private set; }
		public int Index { get; set; }
		public string DisplayNameKey { get; private set; }
		public string TooltipKey { get; private set; }

		[DependsOn(nameof(Page.IsActive))]
		public UserControl? Content
		{
			get
			{
				if (this.control == null)
				{
					if (!this.IsActive)
						return null;

					this.control = this.CreateContent();
				}

				return this.control;
			}
		}

		public IconChar Icon { get; private set; }
		public bool IsActive { get; set; }
		public object? DataContext { get; set; }

		protected abstract UserControl CreateContent();
	}

	public class Page<T> : Page
		where T : UserControl
	{
		public Page(IconChar icon, string name)
			: base(icon, name)
		{
		}

		protected override UserControl CreateContent()
		{
			UserControl? control = Activator.CreateInstance<T>();

			if (control == null)
				throw new Exception($"Failed to create page content: {typeof(T)}");

			return control;
		}
	}
}
