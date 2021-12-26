// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	public partial class ActorAnimationView : UserControl
	{
		public ActorAnimationView()
		{
			this.DataContext = this;
			this.InitializeComponent();
		}

		public ushort AnimationId { get; set; } = 8376;

		private void ApplyAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var selectedActor = TargetService.Instance.SelectedActor;
			if (selectedActor != null)
			{
				AnimationService.Instance.AnimateActor(selectedActor, this.AnimationId);
			}
		}

		private void DisableAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			AnimationService.Instance.Enabled = false;
		}

		private void EnableAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			AnimationService.Instance.Enabled = true;
		}
	}
}