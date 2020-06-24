// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule
{
	using System.Threading.Tasks;
	using ConceptMatrix.AppearanceModule.Pages;
	using ConceptMatrix.Modules;

	public class Module : IModule
	{
		public Task Initialize()
		{
			IViewService viewService = Services.Get<IViewService>();
			viewService.AddActorPage<AppearancePage>("Appearance", "user", this.IsActorSupported);

			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		private bool IsActorSupported(Actor actor)
		{
			return actor.Type == Anamnesis.ActorTypes.Player ||
				actor.Type == Anamnesis.ActorTypes.EventNpc ||
				actor.Type == Anamnesis.ActorTypes.BattleNpc;
		}
	}
}