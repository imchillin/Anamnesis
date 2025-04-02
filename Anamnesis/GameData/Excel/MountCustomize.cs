// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Excel;

/// <summary>
/// Represents the mount actor customization data in the game data.
/// </summary>
[Sheet("MountCustomize", 0x525DEF92)]
public readonly struct MountCustomize(ExcelPage page, uint offset, uint row)
	: IExcelRow<MountCustomize>
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>
	/// Gets the Male Midlander Hyur model mount scaling value.
	/// </summary>
	public readonly ushort HyurMidlanderMaleScale => page.ReadUInt16(offset);

	/// <summary>
	/// Gets the Female Midlander Hyur model mount scaling value.
	/// </summary>
	public readonly ushort HyurMidlanderFemaleScale => page.ReadUInt16(offset + 2);

	/// <summary>
	/// Gets the Male Highlander Hyur model mount scaling value.
	/// </summary>
	public readonly ushort HyurHighlanderMaleScale => page.ReadUInt16(offset + 4);

	/// <summary>
	/// Gets the Female Highlander Hyur model mount scaling value.
	/// </summary>
	public readonly ushort HyurHighlanderFemaleScale => page.ReadUInt16(offset + 6);

	/// <summary>
	/// Gets the Male Elezen model mount scaling value.
	/// </summary>
	public readonly ushort ElezenMaleScale => page.ReadUInt16(offset + 8);

	/// <summary>
	/// Gets the Female Elezen model mount scaling value.
	/// </summary>
	public readonly ushort ElezenFemaleScale => page.ReadUInt16(offset + 10);

	/// <summary>
	/// Gets the Male Lalafell model mount scaling value.
	/// </summary>
	public readonly ushort LalaMaleScale => page.ReadUInt16(offset + 12);

	/// <summary>
	/// Gets the Female Lalafell model mount scaling value.
	/// </summary>
	public readonly ushort LalaFemaleScale => page.ReadUInt16(offset + 14);

	/// <summary>
	/// Gets the Male Miqo'te model mount scaling value.
	/// </summary>
	public readonly ushort MiqoMaleScale => page.ReadUInt16(offset + 16);

	/// <summary>
	/// Gets the Female Miqo'te model mount scaling value.
	/// </summary>
	public readonly ushort MiqoFemaleScale => page.ReadUInt16(offset + 18);

	/// <summary>
	/// Gets the Male Roegadyn model mount scaling value.
	/// </summary>
	public readonly ushort RoeMaleScale => page.ReadUInt16(offset + 20);

	/// <summary>
	/// Gets the Female Roegadyn model mount scaling value.
	/// </summary>
	public readonly ushort RoeFemaleScale => page.ReadUInt16(offset + 22);

	/// <summary>
	/// Gets the Male Au Ra model mount scaling value.
	/// </summary>
	public readonly ushort AuRaMaleScale => page.ReadUInt16(offset + 24);

	/// <summary>
	/// Gets the Female Au Ra model mount scaling value.
	/// </summary>
	public readonly ushort AuRaFemaleScale => page.ReadUInt16(offset + 26);

	/// <summary>
	/// Gets the Male Hrothgar model mount scaling value.
	/// </summary>
	public readonly ushort HrothgarMaleScale => page.ReadUInt16(offset + 28);

	/// <summary>
	/// Gets the Female Hrothgar model mount scaling value.
	/// </summary>
	public readonly ushort HrothgarFemaleScale => page.ReadUInt16(offset + 30);

	/// <summary>
	/// Gets the Male Viera model mount scaling value.
	/// </summary>
	public readonly ushort VieraMaleScale => page.ReadUInt16(offset + 32);

	/// <summary>
	/// Gets the Female Viera model mount scaling value.
	/// </summary>
	public readonly ushort VieraFemaleScale => page.ReadUInt16(offset + 34);

	/// <summary>
	/// Creates a new instance of the <see cref="MountCustomize"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="MountCustomize"/> struct.</returns>
	static MountCustomize IExcelRow<MountCustomize>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
