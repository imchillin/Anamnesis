// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using Anamnesis.Modules;
	using Anamnesis.PoseModule.Pages;
	using Anamnesis.Services;

	public class Module : IModule
	{
		public async Task Initialize()
		{
			await ServiceManager.Add<PoseService>();

			ViewService.AddPage<PosePage>("Pose", "running", this.IsActorPoseSupported);
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		private bool IsActorPoseSupported(ActorViewModel actor)
		{
			////if (actor.ObjectKind != ActorTypes.Player && actor.ObjectKind != ActorTypes.EventNpc && actor.ObjectKind != ActorTypes.BattleNpc)
			////	return false;

			////if (actor.ModelType != 0)
			////	return false;

			return true;
		}
	}
}