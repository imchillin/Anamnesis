// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = SIZE)]
public unsafe struct CharacterDrawData
{
	public const int SIZE = 0x80;
	public const int CUSTOMIZE_SIZE = 32;
	public const int EQUIPMENT_SLOTS = 10;
	public const int UNK_DATA_SIZE = 16;

	/// <summary>
	/// 0x00-0x1F: Customization Data (Race, Gender, Face, Hair, etc.).
	/// </summary>
	public fixed byte Customize[CUSTOMIZE_SIZE];

	/// <summary>
	/// 0x20-0x6F: Equipment Slots (10 slots, 8 bytes each).
	/// Layout per slot: [ushort ModelId][ushort Variant][byte Dye1][byte Dye2][ushort padding].
	/// </summary>
	public fixed ulong Equipment[EQUIPMENT_SLOTS];

	/// <summary>
	/// Seems to change head gear by overlaying on top of the base head model, but
	/// im confused what the purpose is here so avoid writing to it.
	/// </summary>
	public fixed byte Unknown[UNK_DATA_SIZE];

	public readonly ReadOnlySpan<byte> AsSpan()
	{
		fixed (CharacterDrawData* ptr = &this)
		{
			return new ReadOnlySpan<byte>(ptr, SIZE);
		}
	}
}