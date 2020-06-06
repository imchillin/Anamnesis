// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using Anamnesis;
	using Anamnesis.Offsets;

	public interface IInjectionService : IService
	{
		string GamePath { get; }

		IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets);
		IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset<T>[] offsets);
		IMemory<T> GetMemory<T>(IBaseMemoryOffset<T> baseOffset, params IMemoryOffset<T>[] offsets);
	}
}
