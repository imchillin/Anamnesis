// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.Windows.Controls;
	using ConceptMatrix.GUI.Services;

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
		}

		private void OnShowPage(ViewService.Page page)
		{
			this.currentView = page.Instance;
			this.ViewArea.Content = this.currentView;
		}
	}
}
