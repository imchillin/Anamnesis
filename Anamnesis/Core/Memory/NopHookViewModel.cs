// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public class NopHookViewModel : IDisposable
{
	private readonly object lockObject = new();
	private readonly IntPtr address;
	private readonly byte[] originalValue;
	private readonly byte[] nopValue;
	private bool enabled;
	private bool disposed = false;

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

		// Register for process exit event
		AppDomain.CurrentDomain.ProcessExit += this.OnProcessExit;
	}

	public bool Enabled
	{
		get
		{
			lock (this.lockObject)
			{
				return this.enabled;
			}
		}

		set
		{
			this.SetEnabled(value);
		}
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	public void SetEnabled(bool enabled)
	{
		lock (this.lockObject)
		{
			if (enabled == this.enabled)
				return;

			this.enabled = enabled;

			if (enabled)
			{
				// Write Nop
				MemoryService.Write(this.address, this.nopValue, true);
			}
			else
			{
				// Write the original value
				MemoryService.Write(this.address, this.originalValue, true);
			}
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		lock (this.lockObject)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					// Restore the original value if it was NOPed
					if (this.enabled)
					{
						MemoryService.Write(this.address, this.originalValue, true);
					}

					// Unregister from process exit event
					AppDomain.CurrentDomain.ProcessExit -= this.OnProcessExit;
				}

				this.disposed = true;
			}
		}
	}

	private void OnProcessExit(object? sender, EventArgs e)
	{
		// Call Dispose explicitly in case the GC hasn't called it yet
		this.Dispose();
	}
}
