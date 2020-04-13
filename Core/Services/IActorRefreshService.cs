// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public interface IActorRefreshService : IService
	{
		void Refresh(IBaseMemoryOffset offset);
	}
}
