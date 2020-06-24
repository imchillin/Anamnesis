// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System.Threading.Tasks;
	using ConceptMatrix.Localization;
	using ConceptMatrix.Modules;
	using ConceptMatrix.PoseModule.Pages;

	public class Module : IModule
	{
		public async Task Initialize()
		{
			Services.Get<ILocalizationService>().Add("Modules/Pose/Languages/");

			await Services.Add<SkeletonService>();
			await Services.Add<PoseService>();

			IViewService viewService = Services.Get<IViewService>();
			viewService.AddActorPage<PosePage>("Pose", "running", this.IsActorPoseSupported);
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		private bool IsActorPoseSupported(Actor actor)
		{
			return actor.Type == Anamnesis.ActorTypes.Player ||
				actor.Type == Anamnesis.ActorTypes.EventNpc ||
				actor.Type == Anamnesis.ActorTypes.BattleNpc;
		}
	}
}