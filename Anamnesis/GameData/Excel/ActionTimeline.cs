// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Interfaces;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("ActionTimeline", 0x55e1a16f)]
public class ActionTimeline : ExcelRow, IAnimation
{
	public ushort AnimationId => (ushort)this.RowId;
	public byte Type { get; set; }
	public string? Key { get; set; }
	public AnimationMemory.AnimationSlots Slot { get; set; }
	public byte IsLoop { get; set; }

	public string? DisplayName => this.Key;
	public ActionTimeline? Timeline => this;
	public ImageReference? Icon => null;

	public IAnimation.AnimationPurpose Purpose => IAnimation.AnimationPurpose.Raw;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Type = parser.ReadColumn<byte>(0);
		this.Key = parser.ReadString(6);
		this.Slot = (AnimationMemory.AnimationSlots)parser.ReadColumn<byte>(4);
		this.IsLoop = parser.ReadColumn<byte>(16);
	}
}
