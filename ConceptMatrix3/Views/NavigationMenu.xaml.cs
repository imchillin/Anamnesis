// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Windows.Controls;
	using System.Windows.Data;
	using ConceptMatrix.GUI.Services;
	using PropertyChanged;
	using static ConceptMatrix.GUI.Services.ViewService;

	/// <summary>
	/// Interaction logic for NavigationMenu.xaml.
	/// </summary>
	public partial class NavigationMenu : UserControl
	{
		private ViewService viewService;
		private Actor actor;

		public NavigationMenu()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.ViewList.DataContext = this;

			this.viewService = App.Services.Get<ViewService>();
			this.viewService.AddingPage += this.OnAddPage;

			foreach (ViewService.Page page in this.viewService.Pages)
			{
				this.OnAddPage(page);
			}

			this.ViewList.SelectedIndex = 0;
		}

		public event PageEvent SelectPage;

		public ObservableCollection<ViewService.Page> Items { get; set; } = new ObservableCollection<ViewService.Page>();

		private void OnAddPage(ViewService.Page page)
		{
			this.Items.Add(page);
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewService.Page page = (ViewService.Page)this.ViewList.SelectedItem;

			if (!page.Supports(this.actor))
				return;

			this.SelectPage?.Invoke(page);
		}

		private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
		{
			this.actor = this.DataContext as Actor;

			if (this.actor == null)
			{
				this.IsEnabled = false;
				return;
			}

			this.Items.Clear();
			foreach (ViewService.Page page in this.viewService.Pages)
			{
				if (!page.Supports(this.actor))
					continue;

				this.OnAddPage(page);
			}

			this.IsEnabled = true;
		}
	}
}
