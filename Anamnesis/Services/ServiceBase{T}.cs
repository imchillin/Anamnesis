// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;
	using SimpleLog;

	public abstract class ServiceBase<T> : IService
		where T : ServiceBase<T>
	{
		protected static readonly Logger Log = SimpleLog.Log.GetLogger<T>();

		private static T? instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
					throw new Exception($"No service found: {typeof(T)}");

				return instance;
			}
		}

		public virtual Task Initialize()
		{
			instance = (T)this;
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
