// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System;
using System.Runtime.InteropServices;

[Flags]
public enum DrawObjectFlags : byte
{
	None = 0,
	Visible = 1 << 0, // 0x01: Controls DirectX 3D model rendering in CharacterBase::UpdateRender
	InWorld = 1 << 1, // 0x02: Object attached to scene world node
	Loaded = 1 << 3,  // 0x08: Model resources initialized
	Culled = 1 << 6,  // 0x40: Frustum / occlusion culled

	// Combined mask for checking/setting initialized and visible draw objects (0x09: Loaded | Visible)
	ActiveRenderState = Loaded | Visible,
}

[StructLayout(LayoutKind.Explicit, Size = 0x090)]
public struct DrawObject
{
	public const int FLAGS_OFFSET = 0x088;

	[FieldOffset(FLAGS_OFFSET)] public byte Flags;

	public bool IsVisible
	{
		readonly get => (this.Flags & (byte)DrawObjectFlags.ActiveRenderState) == (byte)DrawObjectFlags.ActiveRenderState;
		set => this.Flags = (byte)(value ? this.Flags | (byte)DrawObjectFlags.ActiveRenderState : this.Flags & ~(byte)DrawObjectFlags.ActiveRenderState);
	}
}
