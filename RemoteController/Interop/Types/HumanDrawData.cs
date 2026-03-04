// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = SIZE)]
public unsafe struct HumanDrawData
{
	public const int SIZE = 0x80;
	public const int UNK_DATA_SIZE = 16;

	/// <summary>
	/// 0x00-0x1F: Customization Data (Race, Gender, Face, Hair, etc.).
	/// </summary>
	[FieldOffset(0x00)]
	public CustomizeData Customize;

	/// <summary>
	/// 0x20-0x6F: Equipment Slots (10 slots, 8 bytes each).
	/// Layout per slot: [ushort ModelId][ushort Variant][byte Dye][byte Dye2][ushort padding].
	/// </summary>
	[FieldOffset(0x20)] public ItemModelId Head;
	[FieldOffset(0x28)] public ItemModelId Chest;
	[FieldOffset(0x30)] public ItemModelId Arms;
	[FieldOffset(0x38)] public ItemModelId Legs;
	[FieldOffset(0x40)] public ItemModelId Feet;
	[FieldOffset(0x48)] public ItemModelId Ear;
	[FieldOffset(0x50)] public ItemModelId Neck;
	[FieldOffset(0x58)] public ItemModelId Wrist;
	[FieldOffset(0x60)] public ItemModelId RFinger;
	[FieldOffset(0x68)] public ItemModelId LFinger;

	/// <summary>
	/// Seems to change head gear by overlaying on top of the base head model, but
	/// im confused what the purpose is here so avoid writing to it.
	/// </summary>
	[FieldOffset(0x70)]
	public fixed byte Unknown[UNK_DATA_SIZE];

	public readonly ReadOnlySpan<byte> AsSpan()
	{
		fixed (HumanDrawData* ptr = &this)
		{
			return new ReadOnlySpan<byte>(ptr, SIZE);
		}
	}
}
