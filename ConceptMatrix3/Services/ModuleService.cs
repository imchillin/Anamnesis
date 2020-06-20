// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
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

			DirectoryInfo directoryInfo = new DirectoryInfo(directory);
			FileInfo[] assemblyPaths = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories);
			List<Assembly> assemblies = new List<Assembly>();

			foreach (FileInfo assemblyInfo in assemblyPaths)
			{
				Assembly assembly = Assembly.LoadFrom(assemblyInfo.FullName);
				assemblies.Add(assembly);
				Log.Write("Loaded assembly: " + assemblyInfo.FullName);
			}

			foreach (Assembly assembly in assemblies)
			{
				await this.InitializeModules(assembly);
			}
		}

		private async Task InitializeModules(Assembly targetAssembly)
		{
			string loc = targetAssembly.Location;
			string dir = Path.GetDirectoryName(loc);

			// Don't initialize assemblies in sub directories, just load them.
			if (dir.EndsWith("bin"))
				return;

			try
			{
				foreach (Type type in targetAssembly.GetTypes())
				{
					if (type.IsAbstract || type.IsInterface)
						continue;

					if (typeof(IModule).IsAssignableFrom(type))
					{
						Log.Write("Initialize Module: " + targetAssembly.GetName().Name);
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
