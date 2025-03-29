// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Excel;

[Sheet("MountCustomize", 0x525DEF92)]
public readonly struct MountCustomize(ExcelPage page, uint offset, uint row)
	: IExcelRow<MountCustomize>
{
	public uint RowId => row;

	public readonly ushort HyurMidlanderMaleScale => page.ReadUInt16(offset);
	public readonly ushort HyurMidlanderFemaleScale => page.ReadUInt16(offset + 2);
	public readonly ushort HyurHighlanderMaleScale => page.ReadUInt16(offset + 4);
	public readonly ushort HyurHighlanderFemaleScale => page.ReadUInt16(offset + 6);
	public readonly ushort ElezenMaleScale => page.ReadUInt16(offset + 8);
	public readonly ushort ElezenFemaleScale => page.ReadUInt16(offset + 10);
	public readonly ushort LalaMaleScale => page.ReadUInt16(offset + 12);
	public readonly ushort LalaFemaleScale => page.ReadUInt16(offset + 14);
	public readonly ushort MiqoMaleScale => page.ReadUInt16(offset + 16);
	public readonly ushort MiqoFemaleScale => page.ReadUInt16(offset + 18);
	public readonly ushort RoeMaleScale => page.ReadUInt16(offset + 20);
	public readonly ushort RoeFemaleScale => page.ReadUInt16(offset + 22);
	public readonly ushort AuRaMaleScale => page.ReadUInt16(offset + 24);
	public readonly ushort AuRaFemaleScale => page.ReadUInt16(offset + 26);
	public readonly ushort HrothgarMaleScale => page.ReadUInt16(offset + 28);
	public readonly ushort HrothgarFemaleScale => page.ReadUInt16(offset + 30);
	public readonly ushort VieraMaleScale => page.ReadUInt16(offset + 32);
	public readonly ushort VieraFemaleScale => page.ReadUInt16(offset + 34);

	static MountCustomize IExcelRow<MountCustomize>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
