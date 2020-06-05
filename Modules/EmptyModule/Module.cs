// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.BlankModule
{
	using System.Threading.Tasks;
	using System.Windows;
	using ConceptMatrix.BlankModule.Pages;
	using ConceptMatrix.Modules;

	public class Module : IModule
	{
		public Task Initialize()
		{
			IViewService viewService = Services.Get<IViewService>();
			viewService.AddPage<BlankPage>("Blank Page", "user");

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