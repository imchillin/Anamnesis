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

	/// <summary>
	/// Interaction logic for NavigationMenu.xaml.
	/// </summary>
	public partial class NavigationMenu : UserControl
	{
		private ViewService viewService;

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
		}

		public ObservableCollection<ViewService.Page> Items { get; set; } = new ObservableCollection<ViewService.Page>();

		private void OnAddPage(ViewService.Page page)
		{
			this.Items.Add(page);
		}

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewService.Page item = (ViewService.Page)this.ViewList.SelectedItem;
			this.viewService.ShowPage(item.Name);
		}
	}
}
