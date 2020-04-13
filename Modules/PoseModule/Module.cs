// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System.Threading.Tasks;
	using ConceptMatrix.Modules;

	public class Module : IModule
	{
		public Task Initialize()
		{
			IViewService viewService = Services.Get<IViewService>();
			viewService.AddPage<SimplePosePage>("Character/Simple Pose");

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