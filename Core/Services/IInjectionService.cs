// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.Diagnostics;

	public interface IInjectionService : IService
	{
		IMemory<T> GetMemory<T>(IBaseMemoryOffset baseAddress, params IMemoryOffset[] offsets);

		Process GetGameProcess();
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
