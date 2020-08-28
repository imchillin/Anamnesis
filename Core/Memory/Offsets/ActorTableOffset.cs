// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Offsets
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

		public IMarshaler<byte> GetCountMemory()
		{
			return MarshalerService.Instance.GetMarshaler<byte>(this);
		}

		public byte GetCount()
		{
			using IMarshaler<byte> mem = this.GetCountMemory();
			return mem.Value;
		}

		public IMarshaler<T> GetActorMemory<T>(byte i, params Offset[] offsets)
		{
			return MarshalerService.Instance.GetMarshaler<T>(this.GetBaseOffset(i), offsets);
		}
	}
}
