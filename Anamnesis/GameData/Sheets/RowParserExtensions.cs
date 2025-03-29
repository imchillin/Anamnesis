// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using Lumina.Excel;

public static class ExcelPageExtensions
{
	public static ushort ReadWeaponSet(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)val;
	}

	public static ushort ReadWeaponBase(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 16);
	}

	public static ushort ReadWeaponVariant(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 32);
	}

	public static ushort ReadSet(this ExcelPage self, uint offset) => 0;

	public static ushort ReadBase(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)val;
	}

	public static ushort ReadVariant(this ExcelPage self, uint offset)
	{
		ulong val = self.ReadUInt64(offset);
		return (ushort)(val >> 16);
	}
}
