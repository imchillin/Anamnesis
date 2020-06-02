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
			viewService.AddPage<AppearancePage>("Appearance", "user", false);

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