// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop;

using System;

/// <summary>
/// A generic interface for Reloaded.Hook function hook/wrappers.
/// </summary>
public interface IFunctionHook
{
	/// <summary>
	/// The assigned unique identifier for this hook.
	/// </summary>
	uint Id { get; }

	/// <summary>
	/// The unique delegate key associated with this hook.
	/// </summary>
	string DelegateKey { get; }

	/// <summary>
	/// The target function address.
	/// </summary>
	nint Address { get; }

	/// <summary>
	/// Invokes the original function code that was hooked/wrapped.
	/// </summary>
	/// <param name="argsPayload">
	/// A byte array containing the serialized arguments for the original function.
	/// </param>
	/// <returns>
	/// The serialized return value from the original function.
	/// </returns>
	byte[] InvokeOriginal(byte[] argsPayload);

	/// <summary>
	/// Invokes the original function code that was hooked/wrapped.
	/// </summary>
	/// <param name="argsPayload">
	/// A byte span containing the serialized arguments for the original function.
	/// </param>
	/// <returns>
	/// The serialized return value from the original function.
	/// </returns>
	byte[] InvokeOriginal(ReadOnlySpan<byte> argsPayload);
}

/// <inheritdoc/>
public interface IFunctionHook<TDelegate> : IFunctionHook
	where TDelegate : Delegate
{
	/// <summary>
	/// The original function delegate that was hooked/wrapped.
	/// </summary>
	public TDelegate OriginalFunction { get; }
}