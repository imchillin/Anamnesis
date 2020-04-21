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

	public class ModuleService : IService
	{
		private List<IModule> modules = new List<IModule>();

		public Task Initialize()
		{
			return this.InitializeModules("./Modules/");
		}

		public async Task Start()
		{
			foreach (IModule module in this.modules)
			{
				await module.Start();
			}
		}

		public Task Shutdown()
		{
			return this.ShutdownModules();
		}

		private async Task InitializeModules(string directory)
		{
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			// Hack test
			DirectoryInfo directoryInfo = new DirectoryInfo(directory + "/SaintCoinach/");
			FileInfo[] assemblies = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories);
			foreach (FileInfo assemblyInfo in assemblies)
			{
				Log.Write("Load assembly: " + assemblyInfo.FullName);
				Assembly assembly = Assembly.LoadFrom(assemblyInfo.FullName);
			}

			directoryInfo = new DirectoryInfo(directory);
			assemblies = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories);

			foreach (FileInfo assemblyInfo in assemblies)
			{
				Assembly assembly = Assembly.LoadFrom(assemblyInfo.FullName);
				await this.InitializeModules(assembly);
			}
		}

		private async Task InitializeModules(Assembly targetAssembly)
		{
			try
			{
				foreach (Type type in targetAssembly.GetTypes())
				{
					if (type.IsAbstract || type.IsInterface)
						continue;

					if (typeof(IModule).IsAssignableFrom(type))
					{
						IModule module = (IModule)Activator.CreateInstance(type);
						await module.Initialize();
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
			foreach (IModule module in this.modules)
			{
				await module.Shutdown();
			}
		}
	}
}
