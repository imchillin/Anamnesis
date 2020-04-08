// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using System;
	using ConceptMatrix.Injection;

	public class ActorTableOffset : BaseOffset<int>
	{
		public ActorTableOffset(ulong offset)
			: base(offset)
		{
		}

		public static implicit operator ActorTableOffset(ulong offset)
		{
			return new ActorTableOffset(offset);
		}

		public IMemory<int> GetCountMemory()
		{
			return InjectionService.Instance.GetMemory<int>(this);
		}

		public int GetCount()
		{
			using (IMemory<int> mem = this.GetCountMemory())
			{
				return mem.Value;
			}
		}

		public IMemory<T> GetActorMemory<T>(int i, params Offset[] offsets)
			where T : IEquatable<T>
		{
			BaseOffset addr = new BaseOffset(this.Offsets[0] + (ulong)((i + 1) * 8));
			return InjectionService.Instance.GetMemory<T>(addr, offsets);
		}
	}
}
