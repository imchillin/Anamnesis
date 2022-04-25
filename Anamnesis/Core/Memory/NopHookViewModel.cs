// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public class NopHookViewModel
{
	private readonly IntPtr address;
	private readonly byte[] originalValue;
	private readonly byte[] nopValue;
	private bool value;

	public NopHookViewModel(IntPtr address, int count)
	{
		this.address = address;

		this.originalValue = new byte[count];
		this.nopValue = new byte[count];

		MemoryService.Read(this.address, this.originalValue, this.originalValue.Length);

		for (int i = 0; i < count; i++)
		{
			this.nopValue[i] = 0x90;
		}
	}

	public bool Enabled
	{
		get
		{
			return this.value;
		}

		set
		{
			this.SetEnabled(value);
		}
	}

	public void SetEnabled(bool enabled)
	{
		if (enabled == this.value)
			return;

		this.value = enabled;

		if (enabled)
		{
			// Write Nop
			MemoryService.Write(this.address, this.nopValue);
		}
		else
		{
			// Write the original value
			MemoryService.Write(this.address, this.originalValue);
		}
	}
}
