// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
    using System.Windows.Controls;
    using Anamnesis.Memory;
    using Anamnesis.Services;

    public partial class ActorActionView : UserControl
	{
		public ActorActionView()
		{
			this.DataContext = this;
			this.InitializeComponent();
		}

		public ActorActionMemory.ActionTypes ActionType { get; set; } = ActorActionMemory.ActionTypes.Emote;
		public ushort ActionId { get; set; } = 232;

		private void ApplyAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var selectedActor = TargetService.Instance.SelectedActor;
			if (selectedActor != null)
			{
				ActorActionsService.Instance.SetActorAction(selectedActor, this.ActionType, this.ActionId);
			}
		}
	}
}
