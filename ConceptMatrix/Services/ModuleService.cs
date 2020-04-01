// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Threading.Tasks;
	using ConceptMatrix.Modules;
	using ConceptMatrix.Services;

	using static ConceptMatrix.Modules.ModuleBase;

	public class ModuleService : IService
	{
		private List<ModuleBase> modules = new List<ModuleBase>();

		public Task Initialize(IServices services)
		{
			return this.InitializeModules("./Modules/", services);
		}

		public async Task Start()
		{
			foreach (ModuleBase module in this.modules)
			{
				await module.Start();
			}
		}

		public Task Shutdown()
		{
			return this.ShutdownModules();
		}

		private async Task InitializeModules(string directory, IServices services)
		{
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			DirectoryInfo directoryInfo = new DirectoryInfo(directory);
			FileInfo[] assemblies = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories);

			foreach (FileInfo assemblyInfo in assemblies)
			{
				Assembly assembly = Assembly.LoadFrom(assemblyInfo.FullName);
				await this.InitializeModules(assembly, services);
			}
		}

		private async Task InitializeModules(Assembly targetAssembly, IServices services)
		{
			try
			{
				foreach (Type type in targetAssembly.GetTypes())
				{
					if (type.IsAbstract || type.IsInterface)
						continue;

					if (typeof(ModuleBase).IsAssignableFrom(type))
					{
						ModuleBase module = (ModuleBase)Activator.CreateInstance(type);
						await module.Initialize(services);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception($"Failed to initialize module assembly: {targetAssembly.FullName}", ex));
			}
		}

		private async Task ShutdownModules()
		{
			foreach (ModuleBase module in this.modules)
			{
				await module.Shutdown();
			}
		}
	}
}
