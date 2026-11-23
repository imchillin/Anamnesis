// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Lumina.Excel;

/// <summary>Represents NPC equipment data in the game data.</summary>
[Sheet("NpcEquip", 0x7EAEB95C)]
public readonly struct NpcEquip(ExcelPage page, uint offset, uint row)
	: IExcelRow<NpcEquip>
{
	/// <inheritdoc/>
	public ExcelPage ExcelPage => page;

	/// <inheritdoc/>
	public uint RowOffset => offset;

	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>Gets the model (base, set, variant) identifier of the main hand.</summary>
	public readonly ulong ModelMainHand => page.ReadUInt64(offset);

	/// <summary>Gets the model (base, set, variant) identifier of the off hand.</summary>
	public readonly ulong ModelOffHand => page.ReadUInt64(offset + 8);

	/// <summary>Gets the model (base, set) identifier of the head gear.</summary>
	public readonly uint ModelHead => page.ReadUInt32(offset + 16);

	/// <summary>Gets the model (base, set) identifier of the body gear.</summary>
	public readonly uint ModelBody => page.ReadUInt32(offset + 20);

	/// <summary> Gets the model (base, set) identifier of the hand gear.</summary>
	public readonly uint ModelHands => page.ReadUInt32(offset + 24);

	/// <summary>Gets the model (base, set) identifier of the leg gear.</summary>
	public readonly uint ModelLegs => page.ReadUInt32(offset + 28);

	/// <summary>Gets the model (base, set) identifier of the feet gear.</summary>
	public readonly uint ModelFeet => page.ReadUInt32(offset + 32);

	/// <summary>Gets the model (base, set) identifier of the ears gear.</summary>
	public readonly uint ModelEars => page.ReadUInt32(offset + 36);

	/// <summary>Gets the model (base, set) identifier of the neck gear.</summary>
	public readonly uint ModelNeck => page.ReadUInt32(offset + 40);

	/// <summary>Gets the model (base, set) identifier of the wrists gear.</summary>
	public readonly uint ModelWrists => page.ReadUInt32(offset + 44);

	/// <summary>Gets the model (base, set) identifier of the left ring gear.</summary>
	public readonly uint ModelLeftRing => page.ReadUInt32(offset + 48);

	/// <summary>Gets the model (base, set) identifier of the right ring gear.</summary>
	public readonly uint ModelRightRing => page.ReadUInt32(offset + 52);

	/// <summary>Gets the primary dye channel of the main hand.</summary>
	public readonly RowRef<Stain> DyeMainHand => new(page.Module, (uint)page.ReadUInt8(offset + 60), page.Language);

	/// <summary>Gets the secondary dye channel of the main hand.</summary>
	public readonly RowRef<Stain> Dye2MainHand => new(page.Module, (uint)page.ReadUInt8(offset + 61), page.Language);

	/// <summary>Gets the primary dye channel of the off hand.</summary>
	public readonly RowRef<Stain> DyeOffHand => new(page.Module, (uint)page.ReadUInt8(offset + 62), page.Language);

	/// <summary>Gets the secondary dye channel of the off hand.</summary>
	public readonly RowRef<Stain> Dye2OffHand => new(page.Module, (uint)page.ReadUInt8(offset + 63), page.Language);

	/// <summary>Gets the primary dye channel of the head gear.</summary>
	public readonly RowRef<Stain> DyeHead => new(page.Module, (uint)page.ReadUInt8(offset + 64), page.Language);

	/// <summary>Gets the secondary dye channel of the head gear.</summary>
	public readonly RowRef<Stain> Dye2Head => new(page.Module, (uint)page.ReadUInt8(offset + 74), page.Language);

	/// <summary>Gets the primary dye channel of the body gear.</summary>
	public readonly RowRef<Stain> DyeBody => new(page.Module, (uint)page.ReadUInt8(offset + 65), page.Language);

	/// <summary>Gets the secondary dye channel of the body gear.</summary>
	public readonly RowRef<Stain> Dye2Body => new(page.Module, (uint)page.ReadUInt8(offset + 75), page.Language);

	/// <summary>Gets the primary dye channel of the hands gear.</summary>
	public readonly RowRef<Stain> DyeHands => new(page.Module, (uint)page.ReadUInt8(offset + 66), page.Language);

	/// <summary>Gets the secondary dye channel of the hands gear.</summary>
	public readonly RowRef<Stain> Dye2Hands => new(page.Module, (uint)page.ReadUInt8(offset + 76), page.Language);

	/// <summary>Gets the primary dye channel of the legs gear.</summary>
	public readonly RowRef<Stain> DyeLegs => new(page.Module, (uint)page.ReadUInt8(offset + 67), page.Language);

	/// <summary>Gets the secondary dye channel of the legs gear.</summary>
	public readonly RowRef<Stain> Dye2Legs => new(page.Module, (uint)page.ReadUInt8(offset + 77), page.Language);

	/// <summary>Gets the primary dye channel of the feet gear.</summary>
	public readonly RowRef<Stain> DyeFeet => new(page.Module, (uint)page.ReadUInt8(offset + 68), page.Language);

	/// <summary>Gets the secondary dye channel of the feet gear.</summary>
	public readonly RowRef<Stain> Dye2Feet => new(page.Module, (uint)page.ReadUInt8(offset + 78), page.Language);

	/// <summary>Gets the primary dye channel of the ears gear.</summary>
	public readonly RowRef<Stain> DyeEars => new(page.Module, (uint)page.ReadUInt8(offset + 69), page.Language);

	/// <summary>Gets the secondary dye channel of the ears gear.</summary>
	public readonly RowRef<Stain> Dye2Ears => new(page.Module, (uint)page.ReadUInt8(offset + 79), page.Language);

	/// <summary>Gets the primary dye channel of the neck gear.</summary>
	public readonly RowRef<Stain> DyeNeck => new(page.Module, (uint)page.ReadUInt8(offset + 70), page.Language);

	/// <summary>Gets the secondary dye channel of the neck gear.</summary>
	public readonly RowRef<Stain> Dye2Neck => new(page.Module, (uint)page.ReadUInt8(offset + 80), page.Language);

	/// <summary>Gets the primary dye channel of the writs gear.</summary>
	public readonly RowRef<Stain> DyeWrists => new(page.Module, (uint)page.ReadUInt8(offset + 71), page.Language);

	/// <summary>Gets the secondary dye channel of the wrists gear.</summary>
	public readonly RowRef<Stain> Dye2Wrists => new(page.Module, (uint)page.ReadUInt8(offset + 81), page.Language);

	/// <summary>Gets the primary dye channel of the left ring gear.</summary>
	public readonly RowRef<Stain> DyeLeftRing => new(page.Module, (uint)page.ReadUInt8(offset + 72), page.Language);

	/// <summary>Gets the secondary dye channel of the left ring gear.</summary>
	public readonly RowRef<Stain> Dye2LeftRing => new(page.Module, (uint)page.ReadUInt8(offset + 82), page.Language);

	/// <summary>Gets the primary dye channel of the right ring gear.</summary>
	public readonly RowRef<Stain> DyeRightRing => new(page.Module, (uint)page.ReadUInt8(offset + 73), page.Language);

	/// <summary>Gets the secondary dye channel of the right ring gear.</summary>
	public readonly RowRef<Stain> Dye2RightRing => new(page.Module, (uint)page.ReadUInt8(offset + 83), page.Language);

	/// <summary>
	/// Creates a new instance of the <see cref="NpcEquip"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="NpcEquip"/> struct.</returns>
	static NpcEquip IExcelRow<NpcEquip>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
