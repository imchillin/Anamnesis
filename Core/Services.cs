// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Threading.Tasks;

	public static class Services
	{
		private static IServices services;

		public static T Get<T>()
			where T : IService
		{
			if (services == null)
				throw new Exception("No services provider registered");

			return services.Get<T>();
		}

		public static Task Add<T>()
			where T : IService, new()
		{
			if (services == null)
				throw new Exception("No services provider registered");

			return services.Add<T>();
		}

		public static void RegisterServicesProvider(IServices services)
		{
			if (Services.services != null)
				throw new Exception("Attempt to register multiple services providers");

			Services.services = services;
		}
	}
}
