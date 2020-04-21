// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.Windows.Input;

	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	public partial class SplashWindow
	{
		public SplashWindow()
		{
			this.InitializeComponent();
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				this.DragMove();
			}
		}
	}
}
