// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.IPC;

using System.Runtime.CompilerServices;

/// <summary>
/// Represents a thread-safe pending synchronous request
/// with signaling capability.
/// </summary>
public sealed class PendingRequest<T> : IDisposable
{
	private readonly ManualResetEventSlim signal = new(false, 0);
	private T? result;
	private volatile bool hasResult = false;

	/// <summary>
	/// Blocks the calling thread until the request is
	/// completed or the timeout elapses.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Wait(int timeoutMs)
	{
		return this.signal.Wait(timeoutMs);
	}

	/// <summary>
	/// Sets the result of the request and signals completion.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetResult(T value)
	{
		this.result = value;
		this.hasResult = true;
		this.signal.Set();
	}

	/// <summary>
	/// Attempts to retrieve the result if available.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetResult(out T? value)
	{
		if (this.hasResult)
		{
			value = this.result;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Resets for reuse.
	/// </summary>
	public void Reset()
	{
		this.signal.Reset();
		this.result = default;
		this.hasResult = false;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		this.signal.Dispose();
	}
}
