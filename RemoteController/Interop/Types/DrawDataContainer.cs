// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x268)]
public struct DrawDataContainer
{
	public const int FACEWEAR_DIRTY_FLAG = 0x248;

	[FieldOffset(0x240)] public ushort FacewearId;
	[FieldOffset(FACEWEAR_DIRTY_FLAG)] public byte FacewearDirtyFlag;
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct ItemModelId
{
	[FieldOffset(0)] public ulong Value;

	[FieldOffset(0)] public ushort Id;
	[FieldOffset(2)] public byte Variant;
	[FieldOffset(3)] public byte Dye;
	[FieldOffset(4)] public byte Dye2;
}

public enum WeaponSlot : uint
{
	MainHand = 0,
	OffHand = 1,
}

[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct WeaponModelId
{
	[FieldOffset(0)] public ulong Value;

	[FieldOffset(0)] public ushort Set;
	[FieldOffset(2)] public ushort Base;
	[FieldOffset(4)] public ushort Variant;
	[FieldOffset(6)] public byte Dye;
	[FieldOffset(7)] public byte Dye2;
}
