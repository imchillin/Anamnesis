// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using PropertyChanged;
	using SimpleLog;

	[AddINotifyPropertyChangedInterface]
	public abstract class ServiceBase<T> : IService, INotifyPropertyChanged
		where T : ServiceBase<T>
	{
		protected static readonly Logger Log = SimpleLog.Log.GetLogger<T>();

		private static T? instance;

		public event PropertyChangedEventHandler? PropertyChanged;

		public static T Instance
		{
			get
			{
				if (instance == null)
					throw new Exception($"No service found: {typeof(T)}");

				return instance;
			}
		}

		public static bool Exists => instance != null;

		public bool IsAlive
		{
			get;
			private set;
		}

		public virtual Task Initialize()
		{
			instance = (T)this;
			this.IsAlive = true;
			return Task.CompletedTask;
		}

		public virtual Task Shutdown()
		{
			this.IsAlive = false;
			return Task.CompletedTask;
		}

		public virtual Task Start()
		{
			return Task.CompletedTask;
		}

		protected void RaisePropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}
