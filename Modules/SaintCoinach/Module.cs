// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using System.Threading.Tasks;
	using Anamnesis.Modules;

	public class Module : IModule
	{
		public async Task Initialize()
		{
			await Services.Add<GameDataService>();
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