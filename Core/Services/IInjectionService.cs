// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using ConceptMatrix.Injection;

	public interface IInjectionService : IService
	{
		string GamePath { get; }
		bool ProcessIsAlive { get; }
		IProcess Process { get; }

		IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets);
		IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset<T>[] offsets);
		IMemory<T> GetMemory<T>(IBaseMemoryOffset<T> baseOffset, params IMemoryOffset<T>[] offsets);
	}

	public interface IMemoryOffset
	{
		ulong[] Offsets
		{
			get;
		}

		string Name
		{
			get;
		}
	}

	public interface IMemoryOffset<T> : IMemoryOffset
	{
	}

	public interface IBaseMemoryOffset : IMemoryOffset
	{
	}

	public interface IBaseMemoryOffset<T> : IBaseMemoryOffset
	{
	}
}
