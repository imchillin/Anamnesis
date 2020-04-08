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
	using ConceptMatrix.Services;

	public class InjectionService : IInjectionService
	{
		private ProcessInjection process;
		private Dictionary<Type, Type> memoryTypeLookup = new Dictionary<Type, Type>();
		private bool isActive;

		public static InjectionService Instance
		{
			get;
			private set;
		}

		public Task Initialize(IServices services)
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

			this.process = new ProcessInjection();

			// TODO: allow for process selection
			try
			{
				this.process.OpenProcess("ffxiv_dx11");
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
			newOffsets.Add(new MappedBaseOffset(this.process.Process, (BaseOffset)baseOffset));
			newOffsets.AddRange(offsets);
			return this.GetMemory<T>(newOffsets.ToArray());
		}

		public IMemory<T> GetMemory<T>(params IMemoryOffset[] offsets)
		{
			UIntPtr address = this.process.GetAddress(offsets);

			Type wrapperType = this.GetMemoryType(typeof(T));
			try
			{
				IMemory<T> memory = (IMemory<T>)Activator.CreateInstance(wrapperType, this.process, address);
				return memory;
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		[DllImport("kernel32.dll")]
		internal static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		internal static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

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
					MemoryBase.TickAllActiveMemory();
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Memory thread exception", ex));
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
