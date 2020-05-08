// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using ConceptMatrix.Injection;

	public interface IInjectionService : IService
	{
		string GamePath { get; }
		bool ProcessIsAlive { get; }
		IProcess Process { get; }

		UIntPtr GetAddress(IBaseMemoryOffset offset);
		UIntPtr GetAddress(params IMemoryOffset[] offsets);
		IMemory<T> GetMemory<T>(IBaseMemoryOffset baseAddress, params IMemoryOffset[] offsets);
	}

	public interface IMemoryOffset
	{
		ulong[] Offsets
		{
			get;
		}
	}

	public interface IMemoryOffset<T> : IMemoryOffset
	{
		IMemory<T> GetMemory(IBaseMemoryOffset baseOffset);
		T GetValue(IBaseMemoryOffset offset);
	}

	public interface IBaseMemoryOffset : IMemoryOffset
	{
		IMemory<T> GetMemory<T>(IMemoryOffset<T> offset);
		T GetValue<T>(IMemoryOffset<T> offset);
	}
}
