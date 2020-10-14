// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	#pragma warning disable SA1649

	public enum DataPaths : short
	{
		None = 0,

		MidlanderMasculine = 101,
		MidlanderChild = 104,
		MidlanderFeminine = 201,
		HighlanderMasculine = 301,
		HighlanderFeminine = 401,
		ElezenMasculine = 501,
		ElezenMasculineChild = 504,
		ElezenFeminine = 601,
		ElezenFeminineChild = 604,
		MiqoteMasculine = 701,
		MiqoteFeminine = 801,
		MiqoteChild = 804,
		RoegadynMasculine = 901,
		RoegadynFeminine = 1001,
		LalafellMasculine = 1101,
		LalafellFeminine = 1201,
		AuRaMasculine = 1301,
		AuRaFeminine = 1401,
		Hrothgar = 1501,
		Viera = 1801,
		PadjalMasculine = 9104,
		PadjalFeminine = 9204,
	}

	public static class DataPathsExtensions
	{
		public static string ToDisplayName(this DataPaths self)
		{
			switch (self)
			{
				case DataPaths.None: return "Unknown";

				case DataPaths.MidlanderMasculine: return "Hyur Midlander Masculine";
				case DataPaths.MidlanderChild: return "Hyur Midlander Child";
				case DataPaths.MidlanderFeminine: return "Hyur Midlander Feminine";
				case DataPaths.HighlanderMasculine: return "Hyur Highlander Masculine";
				case DataPaths.HighlanderFeminine: return "Hyur Highlander Feminine";
				case DataPaths.ElezenMasculine: return "Elezen Masculine";
				case DataPaths.ElezenMasculineChild: return "Elezen Masculine Child";
				case DataPaths.ElezenFeminine: return "Elezen Feminine";
				case DataPaths.ElezenFeminineChild: return "Elezen Feminine Child";
				case DataPaths.MiqoteMasculine: return "Miqo'te Masculine";
				case DataPaths.MiqoteFeminine: return "Miqo'te Feminine";
				case DataPaths.MiqoteChild: return "Miqo'te Child";
				case DataPaths.RoegadynMasculine: return "Roegadyn Masculine";
				case DataPaths.RoegadynFeminine: return "Roegadyn Feminine";
				case DataPaths.LalafellMasculine: return "Lalafell Masculine";
				case DataPaths.LalafellFeminine: return "Lalafell Feminine";
				case DataPaths.AuRaMasculine: return "Au Ra Masculine";
				case DataPaths.AuRaFeminine: return "Au Ra Feminine";
				case DataPaths.Hrothgar: return "Hrothgar";
				case DataPaths.Viera: return "Viera";
				case DataPaths.PadjalMasculine: return "Padjal Masculine";
				case DataPaths.PadjalFeminine: return "Padjal Feminine";
			}

			throw new System.Exception($"Unknown data pach key {self}");
		}

		public static byte GetHead(this DataPaths self, Appearance.Tribes tribe)
		{
			if (self == DataPaths.None)
				return 0;

			if ((int)tribe % 2 != 0)
			{
				if (self == DataPaths.HighlanderMasculine || self == DataPaths.HighlanderFeminine)
					return 0x65;

				return 0x01;
			}
			else if ((int)tribe <= 10)
			{
				if (self == DataPaths.MidlanderMasculine || self == DataPaths.MidlanderFeminine)
					return 0x01;

				return 0x65;
			}
			else
			{
				if (self == DataPaths.HighlanderMasculine || self == DataPaths.HighlanderFeminine)
					return 0xC9;

				return 0x65;
			}
		}
	}
}
