// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Pages
{
	using System.Windows.Controls;
	using Anamnesis;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for ActorPage.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class ActorPage : UserControl
	{
		private IMemory<Vector> posMem;
		private IMemory<Quaternion> rotMem;

		public ActorPage()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public Vector Position { get; set; }
		public Quaternion Rotation { get; set; }

		private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
		{
			if (this.posMem != null)
				this.posMem.Dispose();

			if (this.rotMem != null)
				this.rotMem.Dispose();

			Actor actor = this.DataContext as Actor;

			System.Windows.Application.Current.Dispatcher.Invoke(() => { this.IsEnabled = actor != null; });

			if (actor == null)
				return;

			this.posMem = actor.GetMemory(Offsets.Main.Position);
			this.posMem.Bind(this, nameof(this.Position));

			this.rotMem = actor.GetMemory(Offsets.Main.Rotation);
			this.rotMem.Bind(this, nameof(this.Rotation));
		}
	}
}
