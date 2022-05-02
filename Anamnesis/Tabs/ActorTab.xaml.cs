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
using System.Collections.ObjectModel;
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
		this.ContentArea.DataContext = this;

		TargetService.ActorSelected += this.OnActorSelected;

		this.Pages[0].IsActive = true;
	}

	public ObservableCollection<Page> Pages { get; private set; } = new()
	{
		new Page<CharacterPage>(IconChar.UserEdit, "MainWindow.AppearanceTab"),
		new Page<ActionPage>(IconChar.Biking, "MainWindow.PoseTab"),
		new Page<PosePage>(IconChar.Running, "MainWindow.ActionTab"),
	};

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

		public Page(IconChar icon, string hotkey)
		{
			this.Icon = icon;

			HotkeyService.RegisterHotkeyHandler(hotkey, () => this.IsActive = true);
		}

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
		public Page(IconChar icon, string hotkey)
			: base(icon, hotkey)
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
