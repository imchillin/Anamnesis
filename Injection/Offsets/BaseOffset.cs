// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using ConceptMatrix;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Services;

	public class BaseOffset : Offset, IBaseMemoryOffset
	{
		public BaseOffset(ulong offset)
			: base(offset)
		{
		}

		public static implicit operator BaseOffset(ulong offset)
		{
			return new BaseOffset(offset);
		}

		public IMemory<T> GetMemory<T>(IMemoryOffset<T> offset)
		{
			return InjectionService.Instance.GetMemory<T>(this, offset);
		}
	}
}
