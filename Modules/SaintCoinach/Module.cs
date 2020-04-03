// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Modules;

	public class Module : ModuleBase
	{
		public override async Task Initialize(IServices services)
		{
			await base.Initialize(services);
			await services.Add<GameDataService>();
		}

		public override Task Shutdown()
		{
			return Task.CompletedTask;
		}
	}
}