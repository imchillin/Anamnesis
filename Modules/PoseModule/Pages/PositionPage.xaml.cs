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
		private IMemory<Quaternion> rotMem;

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

		public Quaternion Rotation
		{
			get;
			set;
		}

		private void OnSelectionChanged(Selection selection)
		{
			if (this.posMem != null)
				this.posMem.Dispose();

			if (this.rotMem != null)
				this.rotMem.Dispose();

			System.Windows.Application.Current.Dispatcher.Invoke(() => { this.IsEnabled = selection != null; });

			if (selection == null)
				return;

			this.posMem = selection.BaseAddress.GetMemory(Offsets.Position);
			this.posMem.Bind(this, nameof(PositionPage.Position));

			this.rotMem = selection.BaseAddress.GetMemory(Offsets.Rotation);
			this.rotMem.Bind(this, nameof(PositionPage.Rotation));
		}
	}
}
