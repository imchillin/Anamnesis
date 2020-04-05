// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using System;
	using System.Collections.Generic;
	using ConceptMatrix.Services;

	[Serializable]
	public class Offset : IMemoryOffset
	{
		public Offset(params ulong[] offsets)
		{
			this.Offsets = offsets;
		}

		public ulong[] Offsets
		{
			get;
			private set;
		}

		public static implicit operator Offset(ulong offset)
		{
			return new Offset(offset);
		}

		public static implicit operator Offset(ulong[] offsets)
		{
			return new Offset(offsets);
		}

		// Special cast for int form constant values.
		public static implicit operator Offset(int[] offsets)
		{
			List<ulong> longOffsets = new List<ulong>();

			foreach (int offset in offsets)
				longOffsets.Add(Convert.ToUInt64(offset));

			return new Offset(longOffsets.ToArray());
		}
	}
}
