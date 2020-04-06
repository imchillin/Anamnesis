// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using System;
	using ConceptMatrix.Injection;

	public class BaseOffset<T> : BaseOffset
	{
		public BaseOffset(ulong offset)
			: base(offset)
		{
		}

		public static implicit operator BaseOffset<T>(ulong offset)
		{
			return new BaseOffset<T>(offset);
		}

		public IMemory<T> GetMemory()
		{
			return InjectionService.Instance.GetMemory<T>(this);
		}
	}
}
