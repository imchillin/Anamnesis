// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController;

using System;
using System.Runtime.CompilerServices;

/// <summary>
/// A set of utility methods for working with hooks.
/// </summary>
public static class HookUtils
{
	/// <summary>
	/// Generates a unique string key for the specified type
	/// using its fully qualified name if available.
	/// </summary>
	/// <param name="type">
	/// The type for which to generate a key.
	/// </param>
	/// <returns>
	/// A string containing the fully qualified name of the type
	/// if available; otherwise, the simple name of the type.
	/// </returns>
	public static string GetKey(Type type) => type.FullName ?? type.Name;
}

/// <summary>
/// A set of utility methods for packing and unpacking hook message identifiers.
/// </summary>
/// <remarks>
/// Message identifiers with sequence number 0 indicates the origin of the function hook call.
/// If the origin is the remote controller, the call context is an interceptor hook that triggers
/// detours. If the origin is the main application, the call context is a wrapper hook that is
/// invoked directly.
/// </remarks>
public static class HookMessageId
{
	public const uint MAX_HOOK_ID = 0x00FFFFFF;

	/// <summary>
	/// Combines the hook identifier and message sequence number into a single packed identifier.
	/// </summary>
	/// <param name="hookId">The unique identifier of the hook. Only the lower 24 bits are used.</param>
	/// <param name="seqNum">The sequence number of the message (0-255).</param>
	/// <returns>The packed identifier containing both the hook ID and sequence number.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint Pack(uint hookId, byte seqNum) => ((uint)seqNum << 24) | (hookId & MAX_HOOK_ID);

	/// <summary>
	/// Extracts the hook identifier from the packed identifier.
	/// </summary>
	/// <param name="packedId">The packed identifier.</param>
	/// <returns>The unique identifier of the hook.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint GetHookId(uint packedId) => packedId & MAX_HOOK_ID;

	/// <summary>
	/// Extracts the message sequence number from the packed identifier.
	/// </summary>
	/// <param name="packedId">The packed identifier.</param>
	/// <returns>The sequence number of the message (0-255).</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte GetSeqNum(uint packedId) => (byte)(packedId >> 24);
}
