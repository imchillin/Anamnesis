// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using PropertyChanged;

	using Vector = Anamnesis.Memory.Vector;

	/// <summary>
	/// Interaction logic for WorldView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class HomeView : UserControl
	{
		public HomeView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public TerritoryService TerritoryService { get => TerritoryService.Instance; }
		public TimeService TimeService { get => TimeService.Instance; }
		public CameraService CameraService { get => CameraService.Instance; }

		public ActorViewModel? Target { get; set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.SetActor(this.DataContext as ActorViewModel);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.SetActor(this.DataContext as ActorViewModel);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.SetActor(null);
		}

		private void SetActor(ActorViewModel? actor)
		{
			this.Target = actor;
		}
	}
}
