// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	using System.Threading.Tasks;

	public interface IService
	{
		Task Initialize(IServices services);
		Task Start();
		Task Shutdown();
	}
}
