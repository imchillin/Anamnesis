// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop;

using System;

/// <summary>
/// An enum that specifies the dispatch mode for function wrappers.
/// </summary>
public enum DispatchMode : byte
{
	/// <summary>
	/// Executes the wrapper immediately as a detached thread call.
	/// </summary>
	Immediate = 0,

	/// <summary>
	/// Executes the wrapper on the next framework tick.
	/// </summary>
	FrameworkTick = 1,
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

/// <summary>
/// An enum that specifies the type of function hook to create.
/// </summary>
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
/// An enum that specifies the strategy for resolving memory signatures.
/// </summary>
public enum SigResolveStrategy
{
	/// <summary>
	/// Resolves the signature by scanning the module's .text section for a matching byte pattern.
	/// </summary>
	TextScan,

	/// <summary>
	/// Resolves the signature as a static address, with an optional offset.
	/// </summary>
	StaticAddress,

	/// <summary>
	/// Resolves the signature as a vtable entry, with an optional offset.
	/// </summary>
	VTableLookup,
}

/// <summary>
/// Initializes a new instance of the <see cref="FunctionBindAttribute"/> class.
/// </summary>
/// <param name="hookType">The type of hook to create.</param>
/// <param name="signature">The memory signature of the function to hook.</param>
/// <param name="offset">An optional offset to apply after signature resolution.</param>
[AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
public sealed class FunctionBindAttribute(
	string signature,
	int offset = 0,
	SigResolveStrategy strategy = SigResolveStrategy.TextScan) : Attribute
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
	/// Gets the resolution strategy for the specified signature.
	/// The default is <see cref="SigResolveStrategy.TextScan"/>.
	/// </summary>
	public SigResolveStrategy Strategy { get; } = strategy;
}
