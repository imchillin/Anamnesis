// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using ConceptMatrix.Memory.Memory;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	using SysProcess = System.Diagnostics.Process;

	public class AnamnesisService
	{
		public Func<Task<SysProcess>>? SelectProcessCallback;
		public Action<Exception>? ErrorCallback;
		public Action<string>? LogCallback;

		private static AnamnesisService? instance;

		private readonly Dictionary<Type, Type> memoryTypeLookup = new Dictionary<Type, Type>();
		private bool isActive;
		private ulong tickCount = 0;

		public bool ProcessIsAlive
		{
			get;
			private set;
		}

		public IProcess? Process
		{
			get;
			private set;
		}

		public string GamePath
		{
			get
			{
				if (this.Process == null)
					throw new Exception("No game process");

				return Path.GetDirectoryName(this.Process.ExecutablePath) + "\\..\\";
			}
		}

		internal static AnamnesisService Instance
		{
			get
			{
				if (instance == null)
					throw new Exception("No Anamnesis Service");

				return instance;
			}
		}

		public async Task Initialize<TProcess>()
			where TProcess : IProcess
		{
			instance = this;
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

			this.Process = Activator.CreateInstance<TProcess>();

			while (!this.ProcessIsAlive)
			{
				try
				{
					SysProcess[] processes = System.Diagnostics.Process.GetProcesses();
					SysProcess? proc = null;
					foreach (SysProcess process in processes)
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
				catch (Exception ex)
				{
					SysProcess? proc = null;

					if (this.SelectProcessCallback != null)
						proc = await this.SelectProcessCallback.Invoke();

					if (proc == null)
						throw new Exception("Unable to locate FFXIV process", ex);

					this.Process.OpenProcess(proc);
					this.ProcessIsAlive = true;
				}
			}

			new Thread(new ThreadStart(this.TickMemoryThread)).Start();
			new Thread(new ThreadStart(this.ProcessWatcherThread)).Start();
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

		public async Task WaitForMemoryTick()
		{
			// we wait for two ticks since we might be towards the end of a tick,
			// meaning the next tick (+1) will become active without ticking _all_ the memory.
			// wait for +2 guarantees that all currently tracked memory will get a chance to tick
			// before we return.
			ulong targetTick = this.tickCount + 2;

			while (this.tickCount < targetTick)
			{
				await Task.Delay(100);
			}
		}

		internal void OnError(Exception ex)
		{
			this.ErrorCallback?.Invoke(ex);
		}

		internal void OnLog(string message)
		{
			this.LogCallback?.Invoke(message);
		}

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
				while (this.isActive)
				{
					Thread.Sleep(16);

					if (!this.ProcessIsAlive)
						return;

					this.tickCount++;
					MemoryBase.TickAllActiveMemory();
				}

				MemoryBase.DisposeAllMemory();
			}
			catch (Exception ex)
			{
				this.OnError(new Exception("Memory thread exception", ex));
			}
		}

		private void ProcessWatcherThread()
		{
			while (this.isActive && this.Process != null)
			{
				this.ProcessIsAlive = this.Process.IsAlive;

				if (!this.ProcessIsAlive)
				{
					this.OnError(new Exception("FFXIV Process has terminated"));
				}

				Thread.Sleep(100);
			}
		}
	}
}
