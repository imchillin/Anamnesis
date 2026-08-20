// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[Flags]
public enum WeaponStateFlags : ushort
{
	None = 0,
	Hidden = 1 << 1, // 0x02: Weapon is hidden in DrawDataContainer
	Loaded = 1 << 4, // 0x10: Weapon model object loaded
}

[StructLayout(LayoutKind.Explicit, Size = 0x70)]
public struct WeaponData
{
	public const int MODEL_ID_OFFSET = 0x000;
	public const int WEAPON_PTR_OFFSET = 0x008;
	public const int DRAW_OBJECT_OFFSET = 0x018;
	public const int STATE_OFFSET = 0x060;

	[FieldOffset(MODEL_ID_OFFSET)] public WeaponModelId ModelId;
	[FieldOffset(WEAPON_PTR_OFFSET)] public nint WeaponPtr;
	[FieldOffset(DRAW_OBJECT_OFFSET)] public nint DrawObjectPtr;
	[FieldOffset(STATE_OFFSET)] public WeaponStateFlags State;

	public bool IsHidden
	{
		readonly get => this.State.HasFlag(WeaponStateFlags.Hidden);
		set => this.State = value
			? this.State | WeaponStateFlags.Hidden
			: this.State & ~WeaponStateFlags.Hidden;
	}
}

[StructLayout(LayoutKind.Explicit, Size = 0x268)]
public struct DrawDataContainer
{
	public const int MAIN_HAND_OFFSET = 0x010;
	public const int OFF_HAND_OFFSET = 0x080;
	public const int FACEWEAR_DIRTY_FLAG = 0x248;

	[FieldOffset(MAIN_HAND_OFFSET)] public WeaponData MainHand;
	[FieldOffset(OFF_HAND_OFFSET)] public WeaponData OffHand;
	[FieldOffset(0x23E)] public byte DrawFlags;
	[FieldOffset(0x240)] public ushort FacewearId;
	[FieldOffset(FACEWEAR_DIRTY_FLAG)] public byte FacewearDirtyFlag;

	public bool IsHeadgearHidden
	{
		readonly get => (this.DrawFlags & 0x01) == 0x01;
		set => this.DrawFlags = (byte)(value ? this.DrawFlags | 0x01 : this.DrawFlags & ~0x01);
	}
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
