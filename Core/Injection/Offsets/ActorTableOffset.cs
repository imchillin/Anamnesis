// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	public class ActorTableOffset : BaseOffset<byte>
	{
		public ActorTableOffset(ulong offset)
			: base(offset)
		{
		}

		public ActorTableOffset(ulong[] offsets)
			: base(offsets)
		{
		}

		public static implicit operator ActorTableOffset(ulong offset)
		{
			return new ActorTableOffset(offset);
		}

		public BaseOffset GetBaseOffset(byte i)
		{
			return new BaseOffset(this.Offsets[0] + (ulong)((i + 1) * 8));
		}

		public IMemory<byte> GetCountMemory()
		{
			IInjectionService injection = Services.Get<IInjectionService>();
			return injection.GetMemory<byte>(this);
		}

		public byte GetCount()
		{
			using IMemory<byte> mem = this.GetCountMemory();
			return mem.Value;
		}

		public IMemory<T> GetActorMemory<T>(byte i, params Offset[] offsets)
		{
			IInjectionService injection = Services.Get<IInjectionService>();
			return injection.GetMemory<T>(this.GetBaseOffset(i), offsets);
		}
	}
}
