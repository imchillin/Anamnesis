// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using Anamnesis.Memory.Offsets;

	public interface IInjectionService : IService
	{
		string GamePath { get; }

		IMarshaler<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets);
		IMarshaler<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset<T>[] offsets);
		IMarshaler<T> GetMemory<T>(IBaseMemoryOffset<T> baseOffset, params IMemoryOffset<T>[] offsets);

		Task WaitForMemoryTick();
	}
}
