// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System.Threading.Tasks;
	using Anamnesis.Localization;
	using Anamnesis.Memory;
	using Anamnesis.Modules;
	using Anamnesis.PoseModule.Pages;

	public class Module : IModule
	{
		public async Task Initialize()
		{
			await Services.Add<SkeletonService>();
			await Services.Add<PoseService>();

			IViewService viewService = Services.Get<IViewService>();
			viewService.AddPage<PosePage>("Pose", "running", this.IsActorPoseSupported);
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
			if (actor.Type != ActorTypes.Player && actor.Type != ActorTypes.EventNpc && actor.Type != ActorTypes.BattleNpc)
				return false;

			int modelType = actor.GetValue(Offsets.Main.ModelType);
			if (modelType != 0)
				return false;

			return true;
		}
	}
}