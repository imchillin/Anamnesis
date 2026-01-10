// © Anamnesis.
// Licensed under the MIT license.

using Anamnesis.Memory;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Anamnesis.GameData;

/// <summary>
/// Known data paths for character identifiers.
/// </summary>
#pragma warning disable format
public enum DataPaths : short
{
	MidlanderMasculine     = 101,
	MidlanderMasculineNPC  = 104,
	MidlanderFeminine      = 201,
	MidlanderFeminineNPC   = 204,
	HighlanderMasculine    = 301,
	HighlanderMasculineNPC = 304,
	HighlanderFeminine     = 401,
	HighlanderFeminineNPC  = 404,
	ElezenMasculine        = 501,
	ElezenMasculineNPC     = 504,
	ElezenFeminine         = 601,
	ElezenFeminineNPC      = 604,
	MiqoteMasculine        = 701,
	MiqoteMasculineNPC     = 704,
	MiqoteFeminine         = 801,
	MiqoteFeminineNPC      = 804,
	RoegadynMasculine      = 901,
	RoegadynMasculineNPC   = 904,
	RoegadynFeminine       = 1001,
	RoegadynFeminineNPC    = 1004,
	LalafellMasculine      = 1101,
	LalafellMasculineNPC   = 1104,
	LalafellFeminine       = 1201,
	LalafellFeminineNPC    = 1204,
	AuRaMasculine          = 1301,
	AuRaMasculineNPC       = 1304,
	AuRaFeminine           = 1401,
	AuRaFeminineNPC        = 1404,
	HrothgarMasculine      = 1501,
	HrothgarMasculineNPC   = 1504,
	HrothgarFeminine       = 1601,
	HrothgarFeminineNPC    = 1604,
	VieraMasculine         = 1701,
	VieraMasculineNPC      = 1704,
	VieraFeminine          = 1801,
	VieraFeminineNPC       = 1804,
	PadjalMasculine        = 9104,
	PadjalFeminine         = 9204,
}
#pragma warning restore format

public static class DataPathResolver
{
	/// <summary>
	/// A translation map between actor tribe/gender/isNpc and game data path IDs.
	/// </summary>
	private static readonly Dictionary<(ActorCustomizeMemory.Tribes, ActorCustomizeMemory.Genders, bool), DataPaths> s_dataPathMap = new()
	{
		// Hyur: Midlander
		{ (ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.MidlanderMasculine },
		{ (ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.MidlanderMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.MidlanderFeminine },
		{ (ActorCustomizeMemory.Tribes.Midlander, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.MidlanderFeminineNPC },

		// Hyur: Highlander
		{ (ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.HighlanderMasculine },
		{ (ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.HighlanderMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.HighlanderFeminine },
		{ (ActorCustomizeMemory.Tribes.Highlander, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.HighlanderFeminineNPC },

		// Elezen
		{ (ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.ElezenMasculine },
		{ (ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.ElezenMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.ElezenFeminine },
		{ (ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.ElezenFeminineNPC },
		{ (ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.ElezenMasculine },
		{ (ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.ElezenMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.ElezenFeminine },
		{ (ActorCustomizeMemory.Tribes.Duskwight, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.ElezenFeminineNPC },

		// Lalafell
		{ (ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.LalafellMasculine },
		{ (ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.LalafellMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.LalafellFeminine },
		{ (ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.LalafellFeminineNPC },
		{ (ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.LalafellMasculine },
		{ (ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.LalafellMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.LalafellFeminine },
		{ (ActorCustomizeMemory.Tribes.Dunesfolk, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.LalafellFeminineNPC },

		// Miqote
		{ (ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.MiqoteMasculine },
		{ (ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.MiqoteMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.MiqoteFeminine },
		{ (ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.MiqoteFeminineNPC },
		{ (ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.MiqoteMasculine },
		{ (ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.MiqoteMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.MiqoteFeminine },
		{ (ActorCustomizeMemory.Tribes.KeeperOfTheMoon, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.MiqoteFeminineNPC },

		// Roegadyn
		{ (ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.RoegadynMasculine },
		{ (ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.RoegadynMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.RoegadynFeminine },
		{ (ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.RoegadynFeminineNPC },
		{ (ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.RoegadynMasculine },
		{ (ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.RoegadynMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.RoegadynFeminine },
		{ (ActorCustomizeMemory.Tribes.Hellsguard, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.RoegadynFeminineNPC },

		// Au Ra
		{ (ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.AuRaMasculine },
		{ (ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.AuRaMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.AuRaFeminine },
		{ (ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.AuRaFeminineNPC },
		{ (ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.AuRaMasculine },
		{ (ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.AuRaMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.AuRaFeminine },
		{ (ActorCustomizeMemory.Tribes.Xaela, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.AuRaFeminineNPC },

		// Hrothgar
		{ (ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.HrothgarMasculine },
		{ (ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.HrothgarMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.HrothgarFeminine },
		{ (ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.HrothgarFeminineNPC },
		{ (ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.HrothgarMasculine },
		{ (ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.HrothgarMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.HrothgarFeminine },
		{ (ActorCustomizeMemory.Tribes.TheLost, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.HrothgarFeminineNPC },

		// Viera
		{ (ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.VieraMasculine },
		{ (ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.VieraMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.VieraFeminine },
		{ (ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.VieraFeminineNPC },
		{ (ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine, false), DataPaths.VieraMasculine },
		{ (ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine, true),  DataPaths.VieraMasculineNPC },
		{ (ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine, false),  DataPaths.VieraFeminine },
		{ (ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine, true),   DataPaths.VieraFeminineNPC },
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ResolveFacePath(DataPaths raceSexId, ushort faceId)
		 => $"chara/human/c{(short)raceSexId:D4}/obj/face/f{faceId:D4}/material/mt_c{(short)raceSexId:D4}f{faceId:D4}_etc_a.mtrl";

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ResolveHairPath(DataPaths raceSexId, ushort hairId)
		=> $"chara/human/c{(short)raceSexId:D4}/obj/hair/h{hairId:D4}/model/c{(short)raceSexId:D4}h{hairId:D4}_hir.mdl";

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ResolveTailEarsPath(DataPaths raceSexId, ushort tailEarsId)
	{
		string type = "tail";
		string type_preffix = "t";
		string type_suffix = "til";

		if (raceSexId is DataPaths.VieraMasculine or DataPaths.VieraFeminine or DataPaths.VieraMasculineNPC or DataPaths.VieraFeminineNPC)
		{
			type = "zear";
			type_preffix = "z";
			type_suffix = "zer";
		}

		return $"chara/human/c{(short)raceSexId:D4}/obj/{type}/{type_preffix}{tailEarsId:D4}/model/c{(short)raceSexId:D4}{type_preffix}{tailEarsId:D4}_{type_suffix}.mdl";
	}

	public static DataPaths? ToDataPath(ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender, bool isNpc)
		=> s_dataPathMap.TryGetValue((tribe, gender, isNpc), out var dataPath) ? dataPath : null;
}