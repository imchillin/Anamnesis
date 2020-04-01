// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using ConceptMatrix.Services;

	public interface IServices
	{
		T Get<T>()
			where T : IService;
	}
}
