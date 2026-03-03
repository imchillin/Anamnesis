// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x110)]
public unsafe struct ConfigBase
{
	[FieldOffset(0x8)] public nint Listener;
	[FieldOffset(0x14)] public uint ConfigCount;
	[FieldOffset(0x18)] public ConfigEntry* ConfigEntryArray;
}

[StructLayout(LayoutKind.Explicit, Size = 0x450)]
public struct SystemConfig
{
	[FieldOffset(0x0)] public ConfigBase SystemConfigBase;
	[FieldOffset(0x118)] public ConfigBase UiConfig;
	[FieldOffset(0x228)] public ConfigBase UiControlConfig;
	[FieldOffset(0x338)] public ConfigBase UiControlGamepadConfig;
}

[StructLayout(LayoutKind.Explicit, Size = 0x38)]
public unsafe struct ConfigEntry
{
	[FieldOffset(0x10)] public byte* Name; // Null-terminated string
	[FieldOffset(0x18)] public int Type;   // 1:Empty, 2:uint, 3:float, 4:string
	[FieldOffset(0x20)] public ConfigValue Value;
	[FieldOffset(0x28)] public ConfigBase* Owner;
	[FieldOffset(0x30)] public uint Index;
}

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public struct ConfigValue
{
	[FieldOffset(0x0)] public uint UInt;
	[FieldOffset(0x0)] public float Float;
	[FieldOffset(0x0)] public nint String; // UTF-8 string pointer
}
