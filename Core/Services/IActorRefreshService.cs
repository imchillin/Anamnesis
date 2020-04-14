// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public interface IActorRefreshService : IService
	{
		bool IsRefreshing
		{
			get;
		}

		void Refresh(IBaseMemoryOffset offset);
	}
}
