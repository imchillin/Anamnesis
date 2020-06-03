// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	public class BaseOffset<T> : BaseOffset, IBaseMemoryOffset<T>
	{
		public BaseOffset(ulong offset)
			: base(offset)
		{
		}

		public BaseOffset(ulong[] offsets)
			: base(offsets)
		{
		}

		public static implicit operator BaseOffset<T>(ulong offset)
		{
			return new BaseOffset<T>(offset);
		}
	}
}
