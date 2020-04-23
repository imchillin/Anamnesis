// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.Threading.Tasks;

	public delegate void RefreshEvent(IBaseMemoryOffset baseOffset);

	public interface IActorRefreshService : IService
	{
		event RefreshEvent OnRefreshStarting;
		event RefreshEvent OnRefreshComplete;

		bool IsRefreshing
		{
			get;
		}

		void Refresh(IBaseMemoryOffset offset);
		Task RefreshAsync(IBaseMemoryOffset offset);
		void PendingRefreshImmediate();
	}
}
