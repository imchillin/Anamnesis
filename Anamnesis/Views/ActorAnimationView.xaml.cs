// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Windows.Controls;
	using Anamnesis.Services;

	public partial class ActorAnimationView : UserControl
	{
		public ActorAnimationView()
		{
			this.DataContext = this;
			this.AnimationService = AnimationService.Instance;
			this.GPoseService = GposeService.Instance;
			this.InitializeComponent();
		}

		public uint AnimationId { get; set; } = 8047;
		public int RepeatTimer { get; set; } = 0;
		public bool SlowMotion { get; set; } = false;

		public AnimationService AnimationService { get; set; }
		public GposeService GPoseService { get; set; }

		private void ApplyAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.AnimationService = AnimationService.Instance;

			var selectedActor = TargetService.Instance.SelectedActor;
			if (selectedActor != null)
			{
				this.AnimationService.AnimateActor(selectedActor, this.AnimationId, this.SlowMotion, this.RepeatTimer);
			}
		}

		private void IdleAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var selectedActor = TargetService.Instance.SelectedActor;
			if (selectedActor != null)
			{
				this.AnimationService.AnimateActor(selectedActor, 3);
			}
		}

		private void DrawAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var selectedActor = TargetService.Instance.SelectedActor;
			if (selectedActor != null)
			{
				this.AnimationService.AnimateActor(selectedActor, 190);
			}
		}

		private void DisableAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.AnimationService.Enabled = false;
		}

		private void EnableAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.AnimationService.Enabled = true;
		}
	}
}