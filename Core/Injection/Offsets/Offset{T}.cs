// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using System;
	using System.Collections.Generic;
	using ConceptMatrix;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Services;

	public class Offset<T> : Offset, IMemoryOffset<T>
	{
		public Offset(params ulong[] offsets)
			: base(offsets)
		{
		}

		public static implicit operator Offset<T>(ulong offset)
		{
			return new Offset<T>(offset);
		}

		public static implicit operator Offset<T>(ulong[] offsets)
		{
			return new Offset<T>(offsets);
		}

		// Special cast for int form constant values.
		public static implicit operator Offset<T>(int[] offsets)
		{
			List<ulong> longOffsets = new List<ulong>();

			foreach (int offset in offsets)
				longOffsets.Add(Convert.ToUInt64(offset));

			return new Offset<T>(longOffsets.ToArray());
		}

		public IMemory<T> GetMemory(IBaseMemoryOffset baseAddress)
		{
			return InjectionService.Instance.GetMemory<T>(baseAddress, this);
		}

		public T GetValue(IBaseMemoryOffset baseOffset)
		{
			using (IMemory<T> mem = this.GetMemory(baseOffset))
			{
				return mem.Value;
			}
		}
	}
}
