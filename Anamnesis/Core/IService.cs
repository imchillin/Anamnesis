// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System.Threading.Tasks;

public interface IService
{
	Task Initialize();
	Task Start();
	Task Shutdown();
}
