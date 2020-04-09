// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class FlagMemory : MemoryBase<Flag>
	{
		private byte[] on;
		private byte[] off;

		public FlagMemory(ProcessInjection process, UIntPtr address, byte[] on, byte[] off)
			: base(process, address, (ulong)on.Length)
		{
			this.on = on;
			this.off = off;
		}

		protected override Flag Read(ref byte[] data)
		{
			if (Equals(this.on, data))
				return Flag.Enabled;

			return Flag.Disabled;
		}

		protected override void Write(Flag value, ref byte[] data)
		{
			if (value.IsEnabled)
			{
				Array.Copy(this.on, data, (int)this.on.Length);
			}
			else
			{
				Array.Copy(this.off, data, (int)this.off.Length);
			}
		}
	}
}
