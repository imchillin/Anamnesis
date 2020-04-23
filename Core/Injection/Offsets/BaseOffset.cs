// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using ConceptMatrix;
	using ConceptMatrix.Injection;

	public class BaseOffset : Offset, IBaseMemoryOffset
	{
		public BaseOffset(ulong offset)
			: base(offset)
		{
		}

		public BaseOffset(ulong[] offsets)
			: base(offsets)
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

		public T GetValue<T>(IMemoryOffset<T> offset)
		{
			using (IMemory<T> mem = this.GetMemory(offset))
			{
				return mem.Value;
			}
		}
	}
}
