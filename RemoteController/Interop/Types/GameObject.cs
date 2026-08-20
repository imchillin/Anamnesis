// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// A visibility arbitration bitmask for <see cref="GameObject"/>.
/// </summary>
/// <remarks>
/// The game uses this field to determine whether an actor should be rendered or hidden in the scene.
/// Different subsystems (cutscenes, layout streaming, mount/dismount managers, event handlers, etc.)
/// can set individual bits in this field to indicate that the actor should be hidden for the flagged reason.
/// <para>
/// An actor is rendered by the render engine if and only if <see cref="RenderMode"/> evaluates to <see cref="Draw"/> (0),
/// indicating no subsystem is actively requesting the actor to be hidden.
/// </para>
/// </remarks>
[Flags]
public enum RenderModes : ulong
{
	/// <summary>
	/// No subsystem is requesting the object to be hidden. The object is drawn normally.
	/// </summary>
	Draw = 0,

	/// <summary>
	/// Object model resources are unloaded / pending unload.
	/// </summary>
	Unload = 1UL << 1,

	/// <summary>
	/// Object model resources are actively loading.
	/// </summary>
	Load = 1UL << 2,

	/// <summary>
	/// Set by <c>GameObject::DisableDraw</c> when tearing down draw state.
	/// </summary>
	DisableDraw = 1UL << 11,
}

[StructLayout(LayoutKind.Explicit, Size = 0x01A0)]
public struct GameObject
{
	public const int DRAW_OBJECT_OFFSET = 0x0100;
	public const int RENDER_FLAGS_OFFSET = 0x0118;

	[FieldOffset(DRAW_OBJECT_OFFSET)]
	public unsafe DrawObject* ModelObject;

	[FieldOffset(RENDER_FLAGS_OFFSET)]
	public RenderModes RenderMode;
}
