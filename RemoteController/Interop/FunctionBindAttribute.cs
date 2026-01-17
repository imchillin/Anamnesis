// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop;

using System;

/// <summary>
/// An enum that specifies the invocation context of a function hook.
/// Applicable only to <see cref="HookType.Wrapper"/> hooks.
/// </summary>
/// <remarks>
/// <see cref="HookType.Interceptor"/> function hooks are always invoked
/// synchronously within the existing call flow of the original function.
/// </remarks>
public enum HookInvokeContext
{
	/// <summary>
	/// Invoke the function hook at the beginning of a framework
	/// thread frame.
	/// </summary>
	FrameworkThread,

	/// <summary>
	/// Invoke the function hook asynchronously, detached from
	/// the framework thread.
	/// </summary>
	Detached,
}

/// <summary>
/// An enum that specifies the invocation behavior of a function hook.
/// </summary>
public enum HookBehavior
{
	/// <summary>
	/// Invoke the hook before the original function is called.
	/// </summary>
	Before,

	/// <summary>
	/// Invoke the hook after the original function is called.
	/// </summary>
	After,

	/// <summary>
	/// Invoke the hook instead of the original function.
	/// </summary>
	Replace,
}

public enum HookType
{
	/// <summary>
	/// A hook type that wraps the original function call and
	/// allows for execution outside of its normal call flow.
	/// </summary>
	Wrapper,

	/// <summary>
	/// A hook type that directly intercepts calls to the original function
	/// and allows for pre- and post-invocation behavior. See
	/// <see cref="HookBehavior"/> for all available hook behavior options.
	/// </summary>
	Interceptor,
}

/// <summary>
/// Initializes a new instance of the <see cref="FunctionBindAttribute"/> class.
/// </summary>
/// <param name="hookType">The type of hook to create.</param>
/// <param name="signature">The memory signature of the function to hook.</param>
/// <param name="offset">An optional offset to apply after signature resolution.</param>
[AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
public sealed class FunctionBindAttribute(string signature, int offset = 0, HookInvokeContext invokeCtx = HookInvokeContext.Detached) : Attribute
{
	/// <summary>
	/// Gets the memory signature of the function to hook.
	/// </summary>
	public string Signature { get; } = signature;

	/// <summary>
	/// Gets the offset to apply after signature resolution.
	/// </summary>
	public int Offset { get; } = offset;

	/// <summary>
	/// Gets the invocation context for this function hook.
	/// </summary>
	/// <remarks>
	/// See <see cref="HookInvokeContext"/> for more information
	/// on all available hook invocation contexts.
	/// </remarks>
	public HookInvokeContext InvokeContext { get; } = invokeCtx;
}
