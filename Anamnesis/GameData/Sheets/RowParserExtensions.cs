// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using Lumina.Excel;
using System.Runtime.CompilerServices;

public static class ExcelPageExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadWeaponSet(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadWeaponBase(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadWeaponVariant(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 32);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadSet(this ExcelPage _, uint __) => 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadBase(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ReadVariant(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong ConvertToModel(ushort set, ushort baseValue, ushort variant)
	{
		ulong result = set;
		result |= (ulong)baseValue << (set != 0 ? 16 : 0);
		result |= (ulong)variant << (set != 0 ? 32 : 16);
		return result;
	}
}
