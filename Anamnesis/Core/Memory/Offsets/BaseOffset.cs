// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Offsets
{
	using Anamnesis.Memory;

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

		public bool Equals(IBaseMemoryOffset other)
		{
			if (this.Offsets.Length == other.Offsets.Length)
			{
				for (int i = 0; i < this.Offsets.Length; i++)
				{
					if (this.Offsets[i] != other.Offsets[i])
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}
	}
}
