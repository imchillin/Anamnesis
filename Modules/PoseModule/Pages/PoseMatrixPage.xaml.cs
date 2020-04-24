// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Pages
{
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for PoseMatrixPage.xaml.
	/// </summary>
	public partial class PoseMatrixPage : UserControl
	{
		public PoseMatrixPage()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = Module.SkeletonViewModel;
		}
	}
}
