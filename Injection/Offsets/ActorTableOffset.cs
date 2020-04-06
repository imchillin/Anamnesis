// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
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

		public IMemory<T> GetActorMemory<T>(int i, params Offset[] offsets)
		{
			BaseOffset addr = new BaseOffset(this.Offsets[0] + (ulong)((i + 1) * 8));
			return InjectionService.Instance.GetMemory<T>(addr, offsets);
		}
	}
}
