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
			viewService.AddPage<PosePage>("Pose", "running");
			viewService.AddPage<PositionPage>("Positioning", "globe");
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