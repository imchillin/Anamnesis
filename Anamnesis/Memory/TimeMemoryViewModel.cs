// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class TimeMemoryViewModel
	{
		private readonly IntPtr address;
		private readonly byte[] originalValue;
		private readonly byte[] newTimeAsm = new byte[] { 0x49, 0xC7, 0xC1, 0x00, 0x00, 0x00, 0x00 };

		private bool value;

		public TimeMemoryViewModel(IntPtr address)
		{
			this.address = address;

			this.originalValue = new byte[7];

			MemoryService.Read(this.address, this.originalValue, this.originalValue.Length);
		}

		public bool Enabled
		{
			get
			{
				return this.value;
			}

			set
			{
				this.value = value;
				this.SetEnabled(value);
			}
		}

		public void SetEnabled(bool enabled)
		{
			this.value = enabled;

			if (enabled)
			{
				// Write Nop
				MemoryService.Write(this.address, this.newTimeAsm);
			}
			else
			{
				// Write the original value
				MemoryService.Write(this.address, this.originalValue);
			}
		}

		public void SetTime(uint newTime)
		{
			// Write Nop
			MemoryService.Write(this.address + 0x3, BitConverter.GetBytes(newTime));
		}
	}
}
