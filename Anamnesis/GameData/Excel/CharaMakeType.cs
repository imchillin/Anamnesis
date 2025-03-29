// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Excel;
using Serilog;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Lumina.Excel.Sheets.CharaMakeType;

[Sheet("CharaMakeType", 0x80D7DB6D)]
public readonly unsafe struct CharaMakeType(ExcelPage page, uint offset, uint row)
	: IExcelRow<CharaMakeType>
{
	public uint RowId => row;

	public string Name => this.RowId.ToString();

	public readonly RowRef<Race> Race => new(page.Module, (uint)page.ReadInt32(offset + 12392), page.Language);
	public readonly ActorCustomizeMemory.Races CustomizeRace => this.Race.Value.CustomizeRace;
	public readonly RowRef<Tribe> Tribe => new(page.Module, (uint)page.ReadInt32(offset + 12396), page.Language);
	public readonly ActorCustomizeMemory.Tribes CustomizeTribe => this.Tribe.Value.CustomizeTribe;
	public readonly ActorCustomizeMemory.Genders Gender => (ActorCustomizeMemory.Genders)page.ReadInt8(offset + 12400);

	public readonly Collection<byte> Voices => new(page, offset, offset, &VoiceStructCtor, 12);
	public readonly Collection<FacialFeatureOptionStruct> FacialFeatureOption => new(page, offset, offset, &FacialFeatureOptionCtor, 8);

	public List<ImageReference> FacialFeatures
	{
		get
		{
			Log.Verbose("Facial feature count: {0}", this.FacialFeatureOption.Count);
			var result = new List<ImageReference>(this.FacialFeatureOption.Count * 7);
			foreach (var option in this.FacialFeatureOption)
			{
				result.AddRange(
				[
					new ImageReference(option.Option1),
					new ImageReference(option.Option2),
					new ImageReference(option.Option3),
					new ImageReference(option.Option4),
					new ImageReference(option.Option5),
					new ImageReference(option.Option6),
					new ImageReference(option.Option7),
				]);
			}

			return result;
		}
	}

	public readonly Collection<CharaMakeStructStruct> CharaMakeStruct => new(page, offset, offset, &CharaMakeStructCtor, 28);

	public Dictionary<int, CustomizeRange> CustomizeRanges
	{
		get
		{
			Dictionary<int, CustomizeRange> result = new();
			for (int i = 0; i < this.CharaMakeStruct.Count; i++)
			{
				CharaMakeStructStruct charaMakeStruct = this.CharaMakeStruct[i];
				byte subMenuType = charaMakeStruct.SubMenuType;
				if (subMenuType != 0x00 && subMenuType != 0x01 && subMenuType != 0x04)
					continue;

				CustomizeRange range = default;
				range.Min = charaMakeStruct.SubMenuGraphic[0];
				range.Max = charaMakeStruct.SubMenuNum - 1 + range.Min;
				result[(int)charaMakeStruct.Customize] = range;
			}

			return result;
		}
	}

	static CharaMakeType IExcelRow<CharaMakeType>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	private static CharaMakeStructStruct CharaMakeStructCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page, parentOffset, offset + (i * 428));
	private static byte VoiceStructCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => page.ReadUInt8(offset + 11984 + i);
	private static FacialFeatureOptionStruct FacialFeatureOptionCtor(ExcelPage page, uint parentOffset, uint offset, uint i) => new(page, parentOffset, offset + 11996 + (i * 28));
}

public struct CustomizeRange
{
	public int Min;
	public int Max;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool InRange(int num) => num >= this.Min && num <= this.Max;
}