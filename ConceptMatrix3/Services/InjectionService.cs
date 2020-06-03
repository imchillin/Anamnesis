// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Exceptions;
	using ConceptMatrix.GUI.Windows;
	using ConceptMatrix.Injection.Memory;
	using ConceptMatrix.Injection.Offsets;

	public class InjectionService : IInjectionService
	{
		private readonly Dictionary<Type, Type> memoryTypeLookup = new Dictionary<Type, Type>();
		private bool isActive;

		public static InjectionService Instance
		{
			get;
			private set;
		}

		public bool ProcessIsAlive
		{
			get;
			private set;
		}

		public IProcess Process
		{
			get;
			private set;
		}

		public string GamePath
		{
			get
			{
				return Path.GetDirectoryName(this.Process.ExecutablePath) + "\\..\\";
			}
		}

		public Task Initialize()
		{
			Instance = this;
			this.isActive = true;
			this.memoryTypeLookup.Clear();

			// Gets all Memory types (Like IntMemory, FloatMemory) and puts them in the lookup
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in asm.GetTypes())
				{
					if (type.IsAbstract || type.IsInterface)
						continue;

					if (typeof(MemoryBase).IsAssignableFrom(type))
					{
						if (type.BaseType.IsGenericType)
						{
							Type[] generics = type.BaseType.GetGenericArguments();
							if (generics.Length == 1)
							{
								this.memoryTypeLookup.Add(generics[0], type);
							}
						}
					}
				}
			}

#if NO_GAME
			this.Process = new DummyProcess();
#else
			this.Process = new WinProcess();
#endif

			while (!this.ProcessIsAlive)
			{
				try
				{
					Process[] processes = System.Diagnostics.Process.GetProcesses();
					Process proc = null;
					foreach (Process process in processes)
					{
						if (process.ProcessName.ToLower().Contains("ffxiv_dx11"))
						{
							if (proc != null)
								throw new Exception("Multiple processes found");

							proc = process;
						}
					}

					if (proc == null)
						throw new Exception("No process found");

					this.Process.OpenProcess(proc);
					this.ProcessIsAlive = true;
				}
				catch (Exception)
				{
					App.Current.Dispatcher.Invoke(() =>
					{
						Process proc = ProcessSelector.FindProcess();

						if (proc == null)
						{
							App.Current.Shutdown();
							return;
						}

						try
						{
							this.Process.OpenProcess(proc);
							this.ProcessIsAlive = true;
						}
						catch (Exception ex)
						{
							Log.Write(ex);
						}
					});
				}
			}

			return Task.CompletedTask;
		}

		public Task Start()
		{
			new Thread(new ThreadStart(this.TickMemoryThread)).Start();
			new Thread(new ThreadStart(this.ProcessWatcherThread)).Start();
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			this.isActive = false;
			return Task.CompletedTask;
		}

		public IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets)
		{
			List<IMemoryOffset> newOffsets = new List<IMemoryOffset>();
			newOffsets.Add(baseOffset);
			newOffsets.AddRange(offsets);
			return this.GetMemory<T>(newOffsets.ToArray());
		}

		public IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset<T>[] offsets)
		{
			return this.GetMemory<T>(baseOffset, (IMemoryOffset[])offsets);
		}

		public IMemory<T> GetMemory<T>(IBaseMemoryOffset<T> baseOffset, params IMemoryOffset<T>[] offsets)
		{
			return this.GetMemory<T>((IBaseMemoryOffset)baseOffset, (IMemoryOffset[])offsets);
		}

		public IMemory<T> GetMemory<T>(params IMemoryOffset[] offsets)
		{
			Type wrapperType = this.GetMemoryType(typeof(T));
			try
			{
				return (MemoryBase<T>)Activator.CreateInstance(wrapperType, this.Process, offsets);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		/*public UIntPtr GetAddress(IBaseMemoryOffset offset)
		{
			IMemoryOffset newOffset = new MappedBaseOffset(this.Process, (BaseOffset)offset);
			UIntPtr ptr = this.Process.GetAddress(newOffset);

			if (ptr == UIntPtr.Zero)
				throw new InvalidAddressException();

			return ptr;
		}*/

		private Type GetMemoryType(Type type)
		{
			if (!this.memoryTypeLookup.ContainsKey(type))
				throw new Exception($"No memory wrapper for type: {type}");

			return this.memoryTypeLookup[type];
		}

		private void TickMemoryThread()
		{
			try
			{
				IActorRefreshService refreshService = Services.Get<IActorRefreshService>();

				while (this.isActive)
				{
					Thread.Sleep(16);

					if (!this.ProcessIsAlive)
						return;

					while (refreshService.IsRefreshing)
						Thread.Sleep(64);

					MemoryBase.TickAllActiveMemory();
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Memory thread exception", ex));
			}
		}

		private void ProcessWatcherThread()
		{
			while (this.isActive)
			{
				this.ProcessIsAlive = this.Process.IsAlive;

				if (!this.ProcessIsAlive)
				{
					Log.Write(new Exception("FFXIV Process has terminated"), "Injection");
				}

				Thread.Sleep(100);
			}
		}
	}
}
