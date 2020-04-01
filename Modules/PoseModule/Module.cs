// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Modules;
	using ConceptMatrix.Services;

	public class Module : ModuleBase
	{
		public override async Task Initialize(IServices services)
		{
			await base.Initialize(services);

			IViewService viewService = services.Get<IViewService>();
			viewService.AddView<SimplePoseView>("Pose");
		}

		public override Task Shutdown()
		{
			return Task.CompletedTask;
		}
	}
}
