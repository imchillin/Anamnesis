// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using ConceptMatrix.Injection;

	public class ActorTableOffset : BaseOffset<byte>
	{
		public ActorTableOffset(ulong offset)
			: base(offset)
		{
		}

		public static implicit operator ActorTableOffset(ulong offset)
		{
			return new ActorTableOffset(offset);
		}

		public IMemory<byte> GetCountMemory()
		{
			return InjectionService.Instance.GetMemory<byte>(this);
		}

		public byte GetCount()
		{
			using (IMemory<byte> mem = this.GetCountMemory())
			{
				return mem.Value;
			}
		}

		public IMemory<T> GetActorMemory<T>(byte i, params Offset[] offsets)
		{
			BaseOffset addr = new BaseOffset(this.Offsets[0] + (ulong)((i + 1) * 8));
			return InjectionService.Instance.GetMemory<T>(addr, offsets);
		}

		public T GetActorValue<T>(byte i, Offset<T> offset)
		{
			using (IMemory<T> mem = this.GetActorMemory<T>(i, offset))
			{
				return mem.Value;
			}
		}
	}
}
