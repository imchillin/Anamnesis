// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Reflection;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Injection.Memory;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;

	public class InjectionService : IInjectionService
	{
		private ProcessInjection process;

		private Dictionary<Type, Type> memoryTypeLookup = new Dictionary<Type, Type>();

		public OffsetsRoot Offsets
		{
			get;
			private set;
		}

		public Task Initialize(IServices services)
		{
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

			// TODO: allow for region selection
			this.Offsets = OffsetsManager.LoadSettings(OffsetsManager.Regions.Live);

			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public IMemory<T> GetMemory<T>(BaseAddresses baseAddress, params string[] offsets)
		{
			return this.GetMemory<T>(baseAddress.GetOffset(this.Offsets), offsets);
		}

		public IMemory<T> GetMemory<T>(string baseAddress, params string[] offsets)
		{
			List<string> newOffsets = new List<string>();
			newOffsets.Add(this.GetBaseAddress(baseAddress));
			newOffsets.AddRange(offsets);
			return this.GetMemory<T>(newOffsets.ToArray());
		}

		public IMemory<T> GetMemory<T>(params string[] offsets)
		{
			UIntPtr address = this.process.GetAddress(offsets);

			Type wrapperType = this.GetMemoryType(typeof(T));
			IMemory<T> memory = (IMemory<T>)Activator.CreateInstance(wrapperType, this.process, address);

			return memory;
		}

		public string GetBaseAddress(string address)
		{
			return this.process.GetBaseAddress(address);
		}

		private Type GetMemoryType(Type type)
		{
			if (!this.memoryTypeLookup.ContainsKey(type))
				throw new Exception($"No memory wrapper for type: {type}");

			return this.memoryTypeLookup[type];
		}
	}
}
