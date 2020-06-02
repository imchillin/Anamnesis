// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Pages
{
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

		private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			this.SetActor(this.DataContext as Actor);
		}

		private void SetActor(Actor actor)
		{
			if (this.posMem != null)
				this.posMem.Dispose();

			if (this.rotMem != null)
				this.rotMem.Dispose();

			System.Windows.Application.Current.Dispatcher.Invoke(() => { this.IsEnabled = actor != null; });

			if (actor == null)
				return;

			this.posMem = actor.BaseAddress.GetMemory(Offsets.Main.Position);
			this.posMem.Bind(this, nameof(PositionPage.Position));

			this.rotMem = actor.BaseAddress.GetMemory(Offsets.Main.Rotation);
			this.rotMem.Bind(this, nameof(PositionPage.Rotation));
		}
	}
}
