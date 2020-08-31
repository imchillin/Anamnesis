// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System.Threading.Tasks;
	using SimpleLog;

	public abstract class ServiceBase<T> : IService
	{
		protected static readonly Logger Log = SimpleLog.Log.GetLogger<T>();

		public virtual Task Initialize()
		{
			return Task.CompletedTask;
		}

		public virtual Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public virtual Task Start()
		{
			return Task.CompletedTask;
		}
	}
}
