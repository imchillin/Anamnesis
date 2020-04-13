// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.Threading.Tasks;

	public interface IService
	{
		Task Initialize(IServices services);
		Task Start();
		Task Shutdown();
	}
}
