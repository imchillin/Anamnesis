// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	using Anamnesis.Core.Memory;

	public class TimeMemory
	{
		private readonly byte[] originalTimeAsm;

		private readonly byte[] newTimeAsm = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };

		private bool value;

		public TimeMemory()
		{
			this.originalTimeAsm = new byte[this.newTimeAsm.Length];

			MemoryService.Read(AddressService.TimeAsm, this.originalTimeAsm, this.originalTimeAsm.Length);
		}

		public long CurrentTime => MemoryService.Read<long>(AddressService.TimeReal);

		public bool Freeze
		{
			get
			{
				return this.value;
			}

			set
			{
				this.value = value;
				this.SetFrozen(value);
			}
		}

		public void SetFrozen(bool enabled)
		{
			this.value = enabled;

			if (enabled)
			{
				// We disable the game code which updates Eorzea Time
				MemoryService.Write(AddressService.TimeAsm, this.newTimeAsm);
			}
			else
			{
				// We write the Eorzea time update code back
				MemoryService.Write(AddressService.TimeAsm, this.originalTimeAsm);
			}
		}

		public void SetTime(long newTime)
		{
			// As long as Eorzea Time updating is disabled we can just set it directly
			MemoryService.Write(AddressService.TimeReal, BitConverter.GetBytes(newTime));
		}
	}
}
