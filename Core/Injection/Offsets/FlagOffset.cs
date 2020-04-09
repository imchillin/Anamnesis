// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using System;
	using ConceptMatrix.Injection.Memory;

	/// <summary>
	/// Flag offsets are special offsets that indicate a static chunk of memory to be written for either
	/// On or Off states. Flag Offsets cannot be altered otherwise.
	/// </summary>
	public class FlagOffset : BaseOffset
	{
		private byte[] on;
		private byte[] off;

		public FlagOffset(ulong offset, byte[] on, byte[] off)
			: base(offset)
		{
			this.on = on;
			this.off = off;
		}

		public IMemory<Flag> GetMemory()
		{
			UIntPtr address = InjectionService.Instance.GetAddress(this);
			return new FlagMemory(InjectionService.Instance.Process, address, this.on, this.off);
		}
	}
}
