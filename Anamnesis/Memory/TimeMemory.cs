// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

using Anamnesis.Core.Memory;

public class TimeMemory
{
	private readonly NopHookViewModel timeAsmHook;

	private bool value;

	public TimeMemory()
	{
		this.timeAsmHook = new NopHookViewModel(AddressService.TimeAsm, 0x07);
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
			this.timeAsmHook.Enabled = true;
		}
		else
		{
			// We write the Eorzea time update code back
			this.timeAsmHook.Enabled = false;
		}
	}

	public void SetTime(long newTime)
	{
		// As long as Eorzea Time updating is disabled we can just set it directly
		MemoryService.Write(AddressService.TimeReal, BitConverter.GetBytes(newTime), false);
	}
}
