// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using System.Collections.Generic;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;
using LuminaData = Lumina.GameData;

[Sheet("CharaMakeType", columnHash: 0x80d7db6d)]
public class CharaMakeType : ExcelRow
{
	public string Name => this.RowId.ToString();

	public ActorCustomizeMemory.Genders Gender { get; private set; }
	public ActorCustomizeMemory.Races Race { get; private set; }
	public ActorCustomizeMemory.Tribes Tribe { get; private set; }

	public Dictionary<int, CustomizeRange>? CustomizeRanges { get; private set; }

	public int[]? FacialFeatureOptions { get; private set; }
	public List<ImageReference>? FacialFeatures { get; private set; }
	public List<byte>? Voices { get; private set; }

	public override void PopulateData(RowParser parser, LuminaData lumina, Language language)
	{
		this.RowId = parser.RowId;
		this.SubRowId = parser.SubRowId;

		this.Race = (ActorCustomizeMemory.Races)parser.ReadColumn<int>(0);
		this.Tribe = (ActorCustomizeMemory.Tribes)parser.ReadColumn<int>(1);
		this.Gender = (ActorCustomizeMemory.Genders)parser.ReadColumn<sbyte>(2);

		this.FacialFeatureOptions = new int[7 * 8];
		this.FacialFeatures = new List<ImageReference>();

		for (int i = 0; i < 7 * 8; i++)
		{
			this.FacialFeatureOptions[i] = parser.ReadColumn<int>(3291 + i);
			this.FacialFeatures.Add(new ImageReference(this.FacialFeatureOptions[i]));
		}

		this.Voices = new List<byte>();
		for (int i = 0; i < 12; i++)
		{
			byte voice = parser.ReadColumn<byte>(3279 + i);
			this.Voices.Add(voice);
		}

		this.CustomizeRanges = new Dictionary<int, CustomizeRange>();
		for (int i = 0; i < 28; i++)
		{
			int optionType = parser.ReadColumn<byte>(59 + i);
			if (optionType != 0 && optionType != 1 && optionType != 4)
				continue;

			int customizeId = (int)parser.ReadColumn<uint>(171 + i);

			CustomizeRange range = default(CustomizeRange);
			range.Min = parser.ReadColumn<byte>(2999 + i);
			range.Max = parser.ReadColumn<byte>(87 + i) - 1 + range.Min;
			this.CustomizeRanges[customizeId] = range;
		}
	}

	public struct CustomizeRange
	{
		public int Min;
		public int Max;

		public bool InRange(int num)
		{
			return num >= this.Min && num <= this.Max;
		}
	}
}