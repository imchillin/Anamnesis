// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Services
{
	public interface IInjectionService : IService
	{
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
	}

	public interface IBaseMemoryOffset : IMemoryOffset
	{
		IMemory<T> GetMemory<T>(IMemoryOffset<T> offset);
	}
}
