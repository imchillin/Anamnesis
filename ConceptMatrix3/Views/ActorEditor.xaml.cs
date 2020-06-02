// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using ConceptMatrix.GUI.Services;
	using MaterialDesignThemes.Wpf;

	/// <summary>
	/// Interaction logic for ActorEditor.xaml.
	/// </summary>
	public partial class ActorEditor : UserControl
	{
		private readonly ViewService viewService;
		private UserControl currentView;

		public ActorEditor()
		{
			this.InitializeComponent();

			this.viewService = App.Services.Get<ViewService>();
			this.viewService.ShowingPage += this.OnShowPage;

			this.currentView = new HomeView();
			this.ViewArea.Content = this.currentView;
		}

		private void OnShowPage(ViewService.Page page)
		{
			this.currentView = page.Instance;
			this.ViewArea.Content = this.currentView;
		}
	}
}
