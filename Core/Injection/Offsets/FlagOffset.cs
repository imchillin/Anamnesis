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
	public class FlagOffset : BaseOffset<Flag>
	{
		public readonly byte[] On;
		public readonly byte[] Off;

		public FlagOffset(ulong offset, byte[] on, byte[] off)
			: base(offset)
		{
			this.On = on;
			this.Off = off;
		}

		public FlagOffset(ulong[] offsets, byte[] on, byte[] off)
		: base(offsets)
		{
			this.On = on;
			this.Off = off;
		}

		/*public IMemory<Flag> GetMemory()
		{
			UIntPtr address = injection.GetAddress(this);
			return new FlagMemory(injection.Process, address, this.On, this.Off);
		}*/
	}
}
