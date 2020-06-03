// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using ConceptMatrix;

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
	}
}
