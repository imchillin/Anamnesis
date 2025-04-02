// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Excel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Lumina.Excel.Sheets.CharaMakeType;

/// <summary>Represents a unique character type (a combination of attributes) in the game data.</summary>
[Sheet("CharaMakeType", 0x80D7DB6D)]
public readonly unsafe struct CharaMakeType(ExcelPage page, uint offset, uint row)
	: IExcelRow<CharaMakeType>
{
	/// <inheritdoc/>
	public uint RowId => row;

	/// <summary>Gets a string representation of the row identifier.</summary>
	public readonly string Name => this.RowId.ToString();

	/// <summary>
	/// Gets the race reference object associated with the character type.
	/// </summary>
	public readonly RowRef<Race> Race => new(page.Module, (uint)page.ReadInt32(offset + 12392), page.Language);

	/// <summary>
	/// Gets the race customization value associated with the character type.
	/// </summary>
	public readonly ActorCustomizeMemory.Races CustomizeRace => this.Race.Value.CustomizeRace;

	/// <summary>
	/// Gets the tribe reference object associated with the character type.
	/// </summary>
	public readonly RowRef<Tribe> Tribe => new(page.Module, (uint)page.ReadInt32(offset + 12396), page.Language);

	/// <summary>
	/// Gets the tribe customization value associated with the character type.
	/// </summary>
	public readonly ActorCustomizeMemory.Tribes CustomizeTribe => this.Tribe.Value.CustomizeTribe;

	/// <summary>
	/// Gets the gender customization value associated with the character type.
	/// </summary>
	public readonly ActorCustomizeMemory.Genders Gender => (ActorCustomizeMemory.Genders)page.ReadInt8(offset + 12400);

	/// <summary>
	/// Gets the voice collection associated with the character type.
	/// </summary>
	public readonly Collection<byte> Voices => new(page, offset, offset, &VoiceStructCtor, 12);

	/// <summary>
	/// Gets the facial feature option collection associated with the character type.
	/// </summary>
	public readonly Collection<FacialFeatureOptionStruct> FacialFeatureOption => new(page, offset, offset, &FacialFeatureOptionCtor, 8);

	/// <summary>
	/// Gets a collection of facial feature icons associated with the character type.
	/// </summary>
	public List<ImgRef> FacialFeatures
	{
		get
		{
			var result = new List<ImgRef>(this.FacialFeatureOption.Count * 7);
			foreach (var option in this.FacialFeatureOption)
			{
				result.AddRange(
				[
					new ImgRef(option.Option1),
					new ImgRef(option.Option2),
					new ImgRef(option.Option3),
					new ImgRef(option.Option4),
					new ImgRef(option.Option5),
					new ImgRef(option.Option6),
					new ImgRef(option.Option7),
				]);
			}

			return result;
		}
	}

	/// <summary>
	/// Gets the character make structure collection associated with the character type.
	/// </summary>
	public readonly Collection<CharaMakeStructStruct> CharaMakeStruct => new(page, offset, offset, &CharaMakeStructCtor, 28);

	/// <summary>
	/// Gets a dictionary of customization ranges associated with the character type.
	/// </summary>
	public Dictionary<int, CustomizeRange> CustomizeRanges
	{
		get
		{
			Dictionary<int, CustomizeRange> result = [];
			for (int i = 0; i < this.CharaMakeStruct.Count; i++)
			{
				CharaMakeStructStruct charaMakeStruct = this.CharaMakeStruct[i];
				byte subMenuType = charaMakeStruct.SubMenuType;
				if (subMenuType != 0x00 && subMenuType != 0x01 && subMenuType != 0x04)
					continue;

				var min = charaMakeStruct.SubMenuGraphic[0];
				var max = charaMakeStruct.SubMenuNum - 1 + min;
				result[(int)charaMakeStruct.Customize] = new CustomizeRange(min, max);
			}

			return result;
		}
	}

	/// <summary>
	/// Creates a new instance of the <see cref="CharaMakeType"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="CharaMakeType"/> struct.</returns>
	static CharaMakeType IExcelRow<CharaMakeType>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	private static CharaMakeStructStruct CharaMakeStructCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page, parentOffset, offset + (i * 428));
	private static byte VoiceStructCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadUInt8(offset + 11984 + i);
	private static FacialFeatureOptionStruct FacialFeatureOptionCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page, parentOffset, offset + 11996 + (i * 28));
}

/// <summary>Represents a facial feature option structure in the game data.</summary>
public readonly struct CustomizeRange(int min, int max)
{
	/// <summary>Gets the minimum value of the customize range.</summary>
	public readonly int Min = min;

	/// <summary>Gets the maximum value of the customize range.</summary>
	public readonly int Max = max;

	/// <summary>
	/// Determines whether a specified number is within the customize range.
	/// </summary>
	/// <param name="num">The number to check.</param>
	/// <returns>True if the number is within the range; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool InRange(int num) => num >= this.Min && num <= this.Max;
}