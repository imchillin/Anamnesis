// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Modules
{
	using System;
	using System.Threading.Tasks;

	public abstract class ModuleBase
	{
		public static IServices Services { get; private set; }

		/// <summary>
		/// Called when the module is loaded into memory during service initialization.
		/// </summary>
		public virtual Task Initialize(IServices services)
		{
			Services = services;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called once all services are ready.
		/// </summary>
		public virtual Task Start()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called as the application quits.
		/// </summary>
		public virtual Task Shutdown()
		{
			return Task.CompletedTask;
		}
	}
}
