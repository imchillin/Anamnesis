// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.IPC;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Event identifiers for signals sent from
/// the remote controller to the main application.
/// </summary>
public enum EventId : uint
{
	Invalid = 0,
	GposeStateChanged = 1,
	TextInputStateChanged = 2,
}

/// <summary>
/// Flags for event subscription requests.
/// </summary>
/// <remarks>
/// </remarks>
[Flags]
public enum EventSubscriptionFlags : byte
{
	None = 0,

	/// <summary>
	/// Unsubscribe all handlers for this event (sets refcount to 0).
	/// If this flag is not set, then the standard refcount decrement logic applies.
	/// </summary>
	/// <remarks>
	/// Only valid for unsubscription requests. Ignored for subscription requests.
	/// </remarks>
	UnsubscribeAll = 1 << 0,
}

/// <summary>
/// Payload for event (un)subscription requests.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct EventSubscriptionData
{
	public EventId EventId;
	public EventSubscriptionFlags Flags;
}

/// <summary>
/// Payload for gpose state change events.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct GposeStateChangedPayload
{
	public bool IsInGpose;
}

/// <summary>
/// Payload for in-game chat input focus state change events.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TextInputStateChangedPayload
{
	public bool IsActive;
}
