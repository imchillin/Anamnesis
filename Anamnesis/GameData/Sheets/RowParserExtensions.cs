// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using Lumina.Excel;
using System.Runtime.CompilerServices;

public static class ExcelPageExtensions
{
	/// <summary>
	/// Reads a weapon model set from the excel page using the provided offset.
	/// </summary>
	/// <param name="self">The excel page.</param>
	/// <param name="offset">The offset to read from.</param>
	/// <returns>The weapon model set.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadWeaponSet(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)val;
	}

	/// <summary>
	/// Reads a weapon model base from the excel page using the provided offset.
	/// </summary>
	/// <param name="self">The excel page.</param>
	/// <param name="offset">The offset to read from.</param>
	/// <returns>The weapon model base.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadWeaponBase(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 16);
	}

	/// <summary>
	/// Reads a weapon model variant from the excel page using the provided offset.
	/// </summary>
	/// <param name="self">The excel page.</param>
	/// <param name="offset">The offset to read from.</param>
	/// <returns>The weapon model variant.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadWeaponVariant(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 32);
	}

	/// <summary>
	/// Reads a base value from the excel page using the provided offset.
	/// </summary>
	/// <param name="self">The excel page.</param>
	/// <param name="offset">The offset to read from.</param>
	/// <returns>The base value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadBase(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)val;
	}

	/// <summary>
	/// Reads a variant value from the excel page using the provided offset.
	/// </summary>
	/// <param name="self">The excel page.</param>
	/// <param name="offset">The offset to read from.</param>
	/// <returns>The variant value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadVariant(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 16);
	}

	/// <summary>
	/// Converts the split (sub)model components (set, base, variant) into a single (sub)model value.
	/// </summary>
	/// <param name="set">The (sub)model set.</param>
	/// <param name="baseValue">The (sub)model base.</param>
	/// <param name="variant">The (sub)model variant.</param>
	/// <returns>The combined (sub)model value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong ConvertToModel(ushort set, ushort baseValue, ushort variant)
	{
		ulong result = set;
		result |= (ulong)baseValue << (set != 0 ? 16 : 0);
		result |= (ulong)variant << (set != 0 ? 32 : 16);
		return result;
	}
}
