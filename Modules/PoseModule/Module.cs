// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System.Threading.Tasks;
	using ConceptMatrix.Modules;
	using ConceptMatrix.PoseModule.Pages;

	public class Module : IModule
	{
		public Task Initialize()
		{
			IViewService viewService = Services.Get<IViewService>();
			viewService.AddPage<SimplePosePage>("Character/Simple Pose", false);
			viewService.AddPage<PositionPage>("Character/Positioning");

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
	}
}