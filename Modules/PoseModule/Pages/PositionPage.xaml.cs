// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Pages
{
	using System.ComponentModel;
	using System.Windows.Controls;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for PositionPage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class PositionPage : UserControl
	{
		private IMemory<Vector> posMem;

		public PositionPage()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			ISelectionService selectionService = Services.Get<ISelectionService>();
			selectionService.SelectionChanged += this.OnSelectionChanged;
			this.OnSelectionChanged(selectionService.CurrentSelection);
		}

		public Vector Position
		{
			get;
			set;
		}

		private void OnSelectionChanged(Selection selection)
		{
			if (this.posMem != null)
				this.posMem.Dispose();

			App.Current.Dispatcher.Invoke(() => { this.IsEnabled = selection != null; });

			if (selection == null)
				return;

			this.posMem = selection.BaseAddress.GetMemory(Offsets.Position);
			this.posMem.Bind(this, nameof(PositionPage.Position));
		}
	}
}
