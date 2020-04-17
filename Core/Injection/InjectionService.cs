// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Injection.Memory;
	using ConceptMatrix.Injection.Offsets;

	public class InjectionService : IInjectionService
	{
		private Dictionary<Type, Type> memoryTypeLookup = new Dictionary<Type, Type>();
		private bool isActive;

		public static InjectionService Instance
		{
			get;
			private set;
		}

		public static bool ProcessIsAlive
		{
			get;
			private set;
		}

		public ProcessInjection Process
		{
			get;
			private set;
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

			this.Process = new ProcessInjection();

			// TODO: allow for process selection
			try
			{
				this.Process.OpenProcess("ffxiv_dx11");
				ProcessIsAlive = true;
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to find FFXIV process", ex);
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

		public Process GetGameProcess()
		{
			return this.Process.Process;
		}

		public IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets)
		{
			List<IMemoryOffset> newOffsets = new List<IMemoryOffset>();
			newOffsets.Add(new MappedBaseOffset(this.Process.Process, (BaseOffset)baseOffset));
			newOffsets.AddRange(offsets);
			return this.GetMemory<T>(newOffsets.ToArray());
		}

		public IMemory<T> GetMemory<T>(params IMemoryOffset[] offsets)
		{
			UIntPtr address = this.GetAddress(offsets);

			string offsetString = string.Empty;
			foreach (IMemoryOffset offset in offsets)
			{
				offsetString += " " + GetString(offset) + ",";
			}

			offsetString = offsetString.Trim(' ', ',');

			Type wrapperType = this.GetMemoryType(typeof(T));
			try
			{
				MemoryBase<T> memory = (MemoryBase<T>)Activator.CreateInstance(wrapperType, this.Process, address);
				memory.Description = offsetString + " (" + address + ")";
				return memory;
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public UIntPtr GetAddress(IBaseMemoryOffset offset)
		{
			IMemoryOffset newOffset = new MappedBaseOffset(this.Process.Process, (BaseOffset)offset);
			return this.Process.GetAddress(newOffset);
		}

		public UIntPtr GetAddress(params IMemoryOffset[] offsets)
		{
			return this.Process.GetAddress(offsets);
		}

		[DllImport("kernel32.dll")]
		internal static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		internal static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

		private static string GetString(IMemoryOffset offset)
		{
			Type type = offset.GetType();
			string typeName = type.Name;

			if (type.IsGenericType)
			{
				typeName = typeName.Split('`')[0];
				typeName += "<";

				Type[] generics = type.GetGenericArguments();
				for (int i = 0; i < generics.Length; i++)
				{
					if (i > 1)
						typeName += ", ";

					typeName += generics[i].Name;
				}

				typeName += ">";
			}

			string val = string.Empty;
			for (int i = 0; i < offset.Offsets.Length; i++)
			{
				if (i > 1)
					val += ", ";

				val += offset.Offsets[i].ToString("X2");
			}

			return typeName + " [" + val + "]";
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
				IActorRefreshService refreshService = Services.Get<IActorRefreshService>();

				while (this.isActive)
				{
					Thread.Sleep(16);

					if (!ProcessIsAlive)
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
				ProcessIsAlive = this.Process.IsAlive;

				if (!ProcessIsAlive)
				{
					Log.Write(new Exception("FFXIV Process has terminated"), "Injection");
				}
			}
		}

		public class MappedBaseOffset : IBaseMemoryOffset
		{
			public MappedBaseOffset(Process process, BaseOffset offset)
			{
				if (offset.Offsets == null || offset.Offsets.Length <= 0)
					throw new Exception("Invalid base offset");

				this.Offsets = new[] { ((ulong)process.MainModule.BaseAddress.ToInt64()) + offset.Offsets[0] };
			}

			public ulong[] Offsets
			{
				get;
				private set;
			}

			public IMemory<T> GetMemory<T>(IMemoryOffset<T> offset)
			{
				throw new NotImplementedException();
			}

			public T GetValue<T>(IMemoryOffset<T> offset)
			{
				throw new NotImplementedException();
			}
		}
	}
}
