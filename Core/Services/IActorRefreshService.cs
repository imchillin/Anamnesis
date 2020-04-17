// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.Threading.Tasks;

	public interface IActorRefreshService : IService
	{
		bool IsRefreshing
		{
			get;
		}

		void Refresh(IBaseMemoryOffset offset);
		Task RefreshAsync(IBaseMemoryOffset offset);
		void PendingRefreshImmediate();
	}
}
