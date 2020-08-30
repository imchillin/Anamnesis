// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Linq;
	using System.Windows.Controls;
	using System.Windows.Data;
	using Anamnesis.GUI.Services;
	using Anamnesis.Memory;
	using PropertyChanged;

	using Actor = Anamnesis.Actor;

	/// <summary>
	/// Interaction logic for NavigationMenu.xaml.
	/// </summary>
	public partial class NavigationMenu : UserControl
	{
		private ViewService viewService;
		private Actor? actor;
		private ViewService.Page? currentSelection;
		private bool suppressSelectionEvent = false;

		public NavigationMenu()
		{
			this.InitializeComponent();

			this.ViewList.DataContext = this;

			this.viewService = App.Services.Get<ViewService>();
			this.viewService.AddingPage += this.OnAddPage;

			foreach (ViewService.Page page in this.viewService.Pages)
			{
				this.OnAddPage(page);
			}

			this.ViewList.SelectedIndex = 0;
		}

		public event ViewService.PageEvent? SelectPage;

		public ObservableCollection<ViewService.Page> Items { get; set; } = new ObservableCollection<ViewService.Page>();

		private void OnAddPage(ViewService.Page page)
		{
			this.Items.Add(page);
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.suppressSelectionEvent)
				return;

			this.currentSelection = (ViewService.Page)this.ViewList.SelectedItem;

			if (this.currentSelection == null)
				return;

			if (!this.currentSelection.Supports(this.actor))
				return;

			this.SelectPage?.Invoke(this.currentSelection);
		}

		private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
		{
			this.SetActor(this.DataContext as Actor);
		}

		private void SetActor(Actor? actor = null)
		{
			this.actor = actor;
			this.IsEnabled = this.actor != null;

			HashSet<ViewService.Page> oldPages = new HashSet<ViewService.Page>(this.Items);

			List<ViewService.Page> newPages = new List<ViewService.Page>();

			foreach (ViewService.Page page in this.viewService.Pages)
			{
				if (!page.Supports(this.actor))
					continue;

				newPages.Add(page);
			}

			bool changed = true;
			if (oldPages.Count == newPages.Count)
			{
				changed = false;

				foreach (ViewService.Page page in newPages)
				{
					if (oldPages.Contains(page))
						continue;

					changed = true;
					break;
				}
			}

			if (!changed)
				return;

			ViewService.Page? oldSelection = this.currentSelection;
			this.suppressSelectionEvent = true;

			this.Items.Clear();
			foreach (ViewService.Page page in newPages)
			{
				this.Items.Add(page);
			}

			if (oldSelection != null && newPages.Contains(oldSelection))
			{
				this.currentSelection = oldSelection;
				this.ViewList.SelectedItem = this.currentSelection;
				this.SelectPage?.Invoke(oldSelection);
			}
			else
			{
				this.SelectPage?.Invoke(this.Items.FirstOrDefault());
			}

			this.suppressSelectionEvent = false;
		}
	}
}
