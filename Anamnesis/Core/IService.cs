// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System.Threading.Tasks;

public interface IService
{
	public bool IsInitialized { get; }
	public bool IsAlive { get; }

	Task Initialize();
	Task Start();
	Task Shutdown();
}
