// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	public partial class SimplePoseView : UserControl
	{
		public SimplePoseView()
		{
			this.InitializeComponent();

			ISelectionService selectionService = Module.Services.Get<ISelectionService>();
			selectionService.SelectionChanged += this.OnSelectionChanged;

			Application.Current.Exit += this.OnApplicationExiting;

			this.OnSelectionChanged(selectionService.CurrentSelection);
		}

		public SimplePoseViewModel ViewModel { get; set; }

		private void OnSelectionChanged(Selection selection)
		{
			if (selection == null)
				return;

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.ViewModel = new SimplePoseViewModel(selection);
				this.ContentArea.DataContext = this.ViewModel;
			});
		}

		private void OnApplicationExiting(object sender, ExitEventArgs e)
		{
			if (this.ViewModel == null)
				return;

			this.ViewModel.IsEnabled = false;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (this.ViewModel == null)
				return;

			this.ViewModel.IsEnabled = false;
		}
	}
}
