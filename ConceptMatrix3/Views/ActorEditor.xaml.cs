// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.GUI.Services;

	/// <summary>
	/// Interaction logic for ActorEditor.xaml.
	/// </summary>
	public partial class ActorEditor : UserControl
	{
		private UserControl currentView;
		private Dictionary<ViewService.Page, UserControl> pages = new Dictionary<ViewService.Page, UserControl>();

		public ActorEditor()
		{
			this.InitializeComponent();
		}

		private void OnShowPage(ViewService.Page page)
		{
			if (page == null)
			{
				this.currentView = null;
				this.ViewArea.Content = null;
				return;
			}

			if (!this.pages.ContainsKey(page))
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					this.pages.Add(page, page.Create());
				});
			}

			this.currentView = this.pages[page];
			this.ViewArea.Content = this.currentView;
		}
	}
}
