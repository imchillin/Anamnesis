// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("MountCustomize", 0xdde90e69)]
public class MountCustomize : ExcelRow
{
	public ushort HyurMidlanderMaleScale { get; set; }
	public ushort HyurMidlanderFemaleScale { get; set; }
	public ushort HyurHighlanderMaleScale { get; set; }
	public ushort HyurHighlanderFemaleScale { get; set; }
	public ushort ElezenMaleScale { get; set; }
	public ushort ElezenFemaleScale { get; set; }
	public ushort LalaMaleScale { get; set; }
	public ushort LalaFemaleScale { get; set; }
	public ushort MiqoMaleScale { get; set; }
	public ushort MiqoFemaleScale { get; set; }
	public ushort RoeMaleScale { get; set; }
	public ushort RoeFemaleScale { get; set; }
	public ushort AuRaMaleScale { get; set; }
	public ushort AuRaFemaleScale { get; set; }
	public ushort HrothgarMaleScale { get; set; }
	public ushort VieraMaleScale { get; set; }
	public ushort VieraFemaleScale { get; set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.HyurMidlanderMaleScale = parser.ReadColumn<ushort>(1);
		this.HyurMidlanderFemaleScale = parser.ReadColumn<ushort>(2);
		this.HyurHighlanderMaleScale = parser.ReadColumn<ushort>(3);
		this.HyurHighlanderFemaleScale = parser.ReadColumn<ushort>(4);
		this.ElezenMaleScale = parser.ReadColumn<ushort>(5);
		this.ElezenFemaleScale = parser.ReadColumn<ushort>(6);
		this.LalaMaleScale = parser.ReadColumn<ushort>(7);
		this.LalaFemaleScale = parser.ReadColumn<ushort>(8);
		this.MiqoMaleScale = parser.ReadColumn<ushort>(9);
		this.MiqoFemaleScale = parser.ReadColumn<ushort>(10);
		this.RoeMaleScale = parser.ReadColumn<ushort>(11);
		this.RoeFemaleScale = parser.ReadColumn<ushort>(12);
		this.AuRaMaleScale = parser.ReadColumn<ushort>(13);
		this.AuRaFemaleScale = parser.ReadColumn<ushort>(14);
		this.HrothgarMaleScale = parser.ReadColumn<ushort>(15);
		this.VieraMaleScale = parser.ReadColumn<ushort>(16);
		this.VieraFemaleScale = parser.ReadColumn<ushort>(17);
	}
}
