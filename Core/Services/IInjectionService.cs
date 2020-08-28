// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System.Threading.Tasks;
	using ConceptMatrix.Memory;
	using ConceptMatrix.Memory.Offsets;

	public interface IInjectionService : IService
	{
		string GamePath { get; }

		IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets);
		IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset<T>[] offsets);
		IMemory<T> GetMemory<T>(IBaseMemoryOffset<T> baseOffset, params IMemoryOffset<T>[] offsets);

		Task WaitForMemoryTick();
	}
}
