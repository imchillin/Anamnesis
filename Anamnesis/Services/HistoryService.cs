// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.Threading.Tasks;
	using Anamnesis.Keyboard;
	using Anamnesis.Memory;

	public class HistoryService : ServiceBase<HistoryService>
	{
		public override Task Initialize()
		{
			HotkeyService.RegisterHotkeyHandler("System.Undo", this.StepBack);
			HotkeyService.RegisterHotkeyHandler("System.Redo", this.StepForward);

			return base.Initialize();
		}

		private void StepBack()
		{
			ActorMemory? actor = TargetService.Instance.SelectedActor;

			if (actor == null)
				return;

			actor.History.StepBack();
		}

		private void StepForward()
		{
		}
	}
}
