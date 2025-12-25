// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop;

using Reloaded.Hooks.Definitions;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a registered function interceptor
/// using <see cref="Reloaded.Hooks"/> <see cref="IHook{TDelegate}"/>.
/// </summary>
/// <typeparam name="TDelegate">
/// The delegate type representing the function signature to be intercepted.
/// </typeparam>
[RequiresUnreferencedCode("Uses HookDelegateRegistry which requires reflection.")]
[RequiresDynamicCode("Uses HookDelegateRegistry which requires dynamic code.")]
internal sealed class FunctionHook<TDelegate>(uint id, string delegateKey, HookBehavior behavior, nint targetAddress, IHook<TDelegate> hook) : IFunctionHook<TDelegate>, IDisposable
	where TDelegate : Delegate
{
	private readonly IHook<TDelegate> hook = hook;
	private bool isDisposed;

	/// <inheritdoc/>
	public uint Id { get; } = id;

	/// <inheritdoc/>
	public string DelegateKey { get; } = delegateKey;

	/// <summary>
	/// Gets the behavior of the hook detour.
	/// </summary>
	/// <remarks>
	/// See <see cref="HookBehavior"/> for details on possible behaviors.
	/// </remarks>
	public HookBehavior Behavior { get; } = behavior;

	/// <inheritdoc/>
	public nint Address { get; } = targetAddress;

	/// <summary>
	/// Gets a value indicating whether the internal hook is currently enabled.
	/// </summary>
	public bool IsEnabled => this.hook.IsHookEnabled;

	/// <inheritdoc/>
	public TDelegate OriginalFunction => this.hook.OriginalFunction;

	/// <summary>
	/// Enables the hook, allowing it to intercept and
	/// process native function calls.
	/// </summary>
	public void Enable() => this.hook.Enable();

	/// <summary>
	/// Disables the hook, preventing it from intercepting
	/// native function calls.
	/// </summary>
	public void Disable() => this.hook.Disable();

	/// <inheritdoc/>
	public byte[] InvokeOriginal(byte[] argsPayload)
	{
		return this.InvokeOriginal(argsPayload.AsSpan());
	}

	/// <inheritdoc/>
	public byte[] InvokeOriginal(ReadOnlySpan<byte> argsPayload)
	{
		byte[]? result = HookDelegateRegistry.InvokeOriginal(this.Id, argsPayload);
		return result ?? throw new InvalidOperationException($"Handler not found for hook ID: {this.Id}");
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (this.isDisposed)
			return;

		if (disposing)
		{
			this.hook.Disable();
		}
		this.isDisposed = true;
	}
}

/// <summary>
/// Represents a function hook that wraps a native function at a specified memory
/// address, providing direct invocation of the original function via a delegate.
/// </summary>
/// <typeparam name="TDelegate">
/// The delegate type representing the function signature to be wrapped.
/// </typeparam>
[RequiresUnreferencedCode("Uses HookDelegateRegistry which requires reflection.")]
[RequiresDynamicCode("Uses HookDelegateRegistry which requires dynamic code.")]
internal sealed class FunctionWrapper<TDelegate>(uint id, string delegateKey, nint targetAddress, TDelegate wrapper) : IFunctionHook<TDelegate>
	where TDelegate : Delegate
{
	private readonly TDelegate wrapper = wrapper;

	/// <inheritdoc/>
	public uint Id { get; } = id;

	/// <inheritdoc/>
	public string DelegateKey { get; } = delegateKey;

	/// <inheritdoc/>
	public nint Address { get; } = targetAddress;

	/// <inheritdoc/>
	public TDelegate OriginalFunction => this.wrapper;

	/// <inheritdoc/>
	public byte[] InvokeOriginal(byte[] argsPayload)
	{
		return this.InvokeOriginal(argsPayload.AsSpan());
	}

	/// <inheritdoc/>
	public byte[] InvokeOriginal(ReadOnlySpan<byte> argsPayload)
	{
		byte[]? result = HookDelegateRegistry.InvokeOriginal(this.Id, argsPayload);
		return result ?? throw new InvalidOperationException($"Handler not found for hook ID: {this.Id}");
	}
}
