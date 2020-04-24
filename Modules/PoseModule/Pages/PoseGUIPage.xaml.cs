// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	public partial class PoseGuiPage : UserControl
	{
		public PoseGuiPage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = Module.SkeletonViewModel;
		}
	}
}
