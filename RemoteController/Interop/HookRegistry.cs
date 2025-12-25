// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop;

using RemoteController.IPC;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A class for lifecycle management for function hooks.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class HookRegistry
{
	private static readonly Lazy<HookRegistry> s_instance = new(static () => new HookRegistry());
	private static readonly byte[] s_emptyPayload = [];

	private readonly ConcurrentDictionary<uint, IFunctionHook> activeHooks = new();
	private readonly ConcurrentDictionary<string, uint> keyToHookId = new();
	private readonly Reloaded.Hooks.ReloadedHooks reloadedHooks = new();
	
	private uint nextHookID = 1; // Id 0 is reserved for "invalid" hooks.

	private HookRegistry()
	{
	}

	/// <summary>
	/// Gets the singleton instance of the <see cref="HookRegistry"/>.
	/// </summary>
	public static HookRegistry Instance => s_instance.Value;

	/// <summary>
	/// Retrieves the <see cref="FunctionBindAttribute"/> descriptor
	/// for the specified delegate key.
	/// </summary>
	/// <param name="key">
	/// The delegate key to look up. Cannot be null or empty.
	/// </param>
	/// <returns>
	/// The associated <see cref="FunctionBindAttribute"/>, or null if not found.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// Thrown if <paramref name="key"/> is null or empty.
	/// </exception>
	public static FunctionBindAttribute? GetAttribute(string key)
	{
		ArgumentException.ThrowIfNullOrEmpty(key);
		foreach (var (delegateType, attr) in HookDelegateRegistry.GetAll())
		{
			if (HookUtils.GetKey(delegateType) == key)
				return attr;
		}
		return null;
	}

	/// <summary>
	/// Gets the hook instance for the specified hook ID.
	/// </summary>
	/// <param name="hookId">The hook ID to look up.</param>
	/// <returns>
	/// The associated <see cref="IFunctionHook"/>, or null if not found.
	/// </returns>
	public IFunctionHook? GetHook(uint hookId)
	{
		if (this.activeHooks.TryGetValue(hookId, out var hook))
			return hook;

		return null;
	}

	/// <summary>
	/// Registers a new function hook using the provided registration data.
	/// </summary>
	/// <param name="regData">
	/// The hook registration data.
	/// </param>
	/// <returns>
	/// The unique hook ID, or 0 if registration failed.
	/// </returns>
	public uint RegisterHook(HookRegistrationData regData)
	{
		try
		{
			string delegateKey = regData.GetKey();
			ArgumentException.ThrowIfNullOrEmpty(delegateKey);

			var descriptor = GetAttribute(delegateKey);
			if (descriptor == null)
			{
				Log.Error($"Failed to register hook: Delegate {delegateKey} not found");
				return 0;
			}

			if (this.keyToHookId.TryGetValue(delegateKey, out uint existingId))
			{
				Log.Warning($"Failed to register hook: A hook[ID: {existingId}] already exists with delegate key {delegateKey}.");
				return existingId;
			}

			uint hookId = this.AllocateHookId();
			try
			{
				var registeredHook = HookDelegateRegistry.CreateHook(delegateKey, hookId, regData, this.reloadedHooks);
				if (registeredHook == null)
				{
					Log.Error($"Failed to create hook instance for: {delegateKey}");
					return 0;
				}

				this.activeHooks[hookId] = registeredHook;
				this.keyToHookId[delegateKey] = hookId;

				Log.Information($"Registered hook[ID: {hookId}, Key: {delegateKey}] at 0x{regData.Address:X}");
				return hookId;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Failed registration: Hook[ID: {hookId}, Key: {delegateKey}].");
				return 0;
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Unhandled exception during hook registration");
			return 0;
		}
	}

	/// <summary>
	/// Unregisters the hook with the specified ID.
	/// </summary>
	/// <param name="hookId">
	/// The hook ID to unregister.
	/// </param>
	/// <returns>
	/// True if the hook was successfully unregistered; otherwise, false.
	/// </returns>
	public bool UnregisterHook(uint hookId)
	{
		if (!this.activeHooks.TryRemove(hookId, out var hook))
		{
			Log.Warning($"Failed to unregister hook: Unknown hook[ID: {hookId}].");
			return false;
		}

		try
		{
			if (hook is IDisposable disposable)
			{
				disposable.Dispose();
			}

			this.keyToHookId.TryRemove(hook.DelegateKey, out _);

			Log.Information($"Unregistered hook[ID: {hookId}] for {hook.DelegateKey}");
			return true;
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to unregister hook[ID: {hookId}.");
			return false;
		}
	}

	/// <summary>
	/// Unregisters all currently registered hooks.
	/// </summary>
	public void UnregisterAll()
	{
		foreach (var hookId in this.activeHooks.Keys.ToArray())
			this.UnregisterHook(hookId);
	}

	/// <summary>
	/// Invokes the original function for the specified hook ID
	/// with the given payload.
	/// </summary>
	/// <param name="hookId">The hook ID to invoke.</param>
	/// <param name="argsPayload">
	/// The serialized argument payload for the original function.
	/// </param>
	/// <returns>
	/// The result payload from the original function, or an empty
	/// array if invocation fails.
	/// </returns>
	public byte[] InvokeOriginal(uint hookId, ReadOnlySpan<byte> argsPayload)
	{
		if (!this.activeHooks.TryGetValue(hookId, out var hook))
		{
			Log.Verbose($"Failed to handle invoke request: Unknown hook[ID: {hookId}].");
			return s_emptyPayload;
		}

		try
		{
			return hook.InvokeOriginal(argsPayload);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to handle invoke request: hook[ID: {hookId}].");
			return s_emptyPayload;
		}
	}

	private uint AllocateHookId()
	{
		uint startId = this.nextHookID;

		do
		{
			uint candidate = this.nextHookID++;

			if (this.nextHookID >= HookMessageId.MAX_HOOK_ID)
				this.nextHookID = 1; // Skip 0 (Reserved for invalid hooks)

			if (!this.activeHooks.ContainsKey(candidate))
				return candidate;

		} while (this.nextHookID != startId);

		throw new InvalidOperationException("No more hook IDs available.");
	}
}
