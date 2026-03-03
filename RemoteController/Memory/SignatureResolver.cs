// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Memory;

using RemoteController;
using RemoteController.Interop;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

/// <summary>
/// An in-process memory signature resolver for the controller's function hook delegates.
/// </summary>
[RequiresUnreferencedCode("Reflection is used within the signature resolver")]
public sealed class SignatureResolver
{
	private readonly SignatureScanner scanner;
	private readonly IProcessMemoryReader memoryReader;
	private readonly ConcurrentDictionary<string, nint> resolvedAddresses = new();
	private readonly ConcurrentDictionary<string, Type> delegateTypeCache = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="SignatureResolver"/> class.
	/// </summary>
	/// <param name="scanner">The signature scanner to use for memory scanning.</param>
	/// <param name="memoryReader">The memory reader for pointer dereferencing.</param>
	public SignatureResolver(SignatureScanner scanner, IProcessMemoryReader memoryReader)
	{
		ArgumentNullException.ThrowIfNull(scanner, nameof(scanner));
		ArgumentNullException.ThrowIfNull(memoryReader, nameof(memoryReader));

		this.scanner = scanner;
		this.memoryReader = memoryReader;
		this.CacheDelegateTypes();
	}

	/// <summary>
	/// Resolves the address for a delegate type identified by its key.
	/// </summary>
	/// <param name="delegateKey">The unique key identifying the delegate type.</param>
	/// <returns>The resolved memory address, or <see cref="nint.Zero"/> if resolution fails.</returns>
	public nint Resolve(string delegateKey)
	{
		if (this.resolvedAddresses.TryGetValue(delegateKey, out nint cached))
			return cached;

		if (!this.delegateTypeCache.TryGetValue(delegateKey, out Type? delegateType))
		{
			Log.Error($"Delegate type not found for key: {delegateKey}");
			return nint.Zero;
		}

		var attr = delegateType.GetCustomAttribute<FunctionBindAttribute>();
		if (attr == null)
		{
			Log.Error($"FunctionBindAttribute not found on delegate: {delegateKey}");
			return nint.Zero;
		}

		return this.Resolve(delegateKey, attr);
	}

	/// <summary>
	/// Resolves the address using the provided attribute configuration.
	/// </summary>
	/// <param name="delegateKey">The delegate key for caching purposes.</param>
	/// <param name="attr">The function bind attribute containing resolution parameters.</param>
	/// <returns>The resolved memory address, or <see cref="nint.Zero"/> if resolution fails.</returns>
	public nint Resolve(string delegateKey, FunctionBindAttribute attr)
	{
		if (this.resolvedAddresses.TryGetValue(delegateKey, out nint cached))
			return cached;

		try
		{
			nint address = attr.Strategy switch
			{
				SigResolveStrategy.TextScan => this.ResolveTextScan(attr),
				SigResolveStrategy.StaticAddress => this.ResolveStaticAddress(attr),
				SigResolveStrategy.VTableLookup => this.ResolveVTableLookup(attr),
				_ => throw new NotSupportedException($"Unknown resolution strategy: {attr.Strategy}"),
			};

			if (address != nint.Zero)
			{
				this.resolvedAddresses[delegateKey] = address;
				Log.Debug($"Resolved [{delegateKey}] -> 0x{address:X} using {attr.Strategy}");
			}

			return address;
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to resolve signature for {delegateKey}: {attr.Signature}");
			return nint.Zero;
		}
	}

	/// <summary>
	/// Attempts to get a cached delegate type by its key.
	/// </summary>
	/// <param name="delegateKey">The delegate key.</param>
	/// <param name="delegateType">The resolved delegate type, if found.</param>
	/// <returns>True if the delegate type was found; otherwise, false.</returns>
	public bool TryGetDelegateType(string delegateKey, out Type? delegateType)
	{
		return this.delegateTypeCache.TryGetValue(delegateKey, out delegateType);
	}

	/// <summary>
	/// Clears all cached resolutions, forcing re-resolution on next access.
	/// </summary>
	public void ClearCache()
	{
		this.resolvedAddresses.Clear();
	}

	private nint ResolveTextScan(FunctionBindAttribute attr)
	{
		return this.scanner.ScanText(attr.Signature) + attr.Offset;
	}

	private nint ResolveStaticAddress(FunctionBindAttribute attr)
	{
		return this.scanner.GetStaticAddressFromSig(attr.Signature, attr.Offset);
	}

	private nint ResolveVTableLookup(FunctionBindAttribute attr)
	{
		// Get base vtable address
		nint vtableBase = this.scanner.GetStaticAddressFromSig(attr.Signature);

		// Offset to function in vtable
		nint vtableSlot = vtableBase + attr.Offset;

		// Dereference to get the actual function pointer
		return this.memoryReader.ReadPtr(vtableSlot);
	}

	private void CacheDelegateTypes()
	{
		var assembly = typeof(SignatureResolver).Assembly;
		var delegateTypes = assembly.GetTypes()
			.Where(t => t.IsSubclassOf(typeof(Delegate)))
			.Where(t => t.GetCustomAttribute<FunctionBindAttribute>() != null);

		foreach (var type in delegateTypes)
		{
			string key = HookUtils.GetKey(type);
			this.delegateTypeCache[key] = type;
			Log.Verbose($"Cached delegate type: {key}");
		}

		Log.Debug($"Cached {this.delegateTypeCache.Count} delegate types for signature resolution.");
	}
}