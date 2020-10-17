// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	#pragma warning disable SA1649

	public enum DataPaths : short
	{
		None = 0,

		MidlanderMasculine = 101,
		MidlanderMasculineChild = 104,
		MidlanderFeminine = 201,
		MidlanderFeminineChild = 204,
		HighlanderMasculine = 301,
		HighlanderFeminine = 401,
		ElezenMasculine = 501,
		ElezenMasculineChild = 504,
		ElezenFeminine = 601,
		ElezenFeminineChild = 604,
		MiqoteMasculine = 701,
		MiqoteMasculineChild = 704,
		MiqoteFeminine = 801,
		MiqoteFeminineChild = 804,
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
