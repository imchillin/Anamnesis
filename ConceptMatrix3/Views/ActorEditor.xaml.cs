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
		private UserControl currentView;

		public ActorEditor()
		{
			this.InitializeComponent();
		}

		private void OnShowPage(ViewService.Page page)
		{
			this.currentView = page.Instance;
			this.ViewArea.Content = this.currentView;
		}
	}
}
