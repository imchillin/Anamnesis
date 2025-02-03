// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public class NopHook : IDisposable
{
	private readonly object lockObject = new();
	private readonly IntPtr address;
	private readonly byte[] originalValue;
	private readonly byte[] nopValue;
	private bool enabled;
	private bool disposed = false;

	public NopHook(IntPtr address, int count)
	{
		this.address = address;

		this.originalValue = new byte[count];
		this.nopValue = new byte[count];

		MemoryService.Read(this.address, this.originalValue, this.originalValue.Length);

		for (int i = 0; i < count; i++)
		{
			this.nopValue[i] = 0x90;
		}

		// Register events to handle normal process termination and fatal exceptions
		AppDomain.CurrentDomain.ProcessExit += this.OnProcessExit;
		AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
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

					// Unregister events
					AppDomain.CurrentDomain.ProcessExit -= this.OnProcessExit;
					AppDomain.CurrentDomain.UnhandledException -= this.OnUnhandledException;
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

	private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
	{
		// Call Dispose explicitly when an unhandled exception occurs to restore the original memory state
		this.Dispose();
	}
}
