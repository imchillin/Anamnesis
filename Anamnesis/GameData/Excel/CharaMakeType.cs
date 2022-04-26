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

	public int[]? FacialFeatureOptions { get; private set; }
	public List<ImageReference>? FacialFeatures { get; private set; }

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
	}
}
