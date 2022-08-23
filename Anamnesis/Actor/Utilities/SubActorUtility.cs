// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Utilities;

using System;
using System.Threading.Tasks;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using Serilog;

public class SubActorUtility
{
	public static async Task SwitchMount(ActorMemory memory, Mount mount)
	{
		if (memory.IsMounted == true && memory.Mount != null)
		{
			memory.MountId = (ushort)mount.RowId;

			float mountScale = CalculateMountScale(mount, memory);
			memory.Mount.Scale = mountScale;

			await Apply(mount, memory.Mount);

			await memory.RefreshAsync();
		}
		else
		{
			Log.Error("Attempted to switch mount when character is not mounted");
		}
	}

	public static async Task SwitchCompanion(ActorMemory memory, Companion companion)
	{
		if (memory.Companion != null)
		{
			memory.Companion.DataId = companion.RowId;
			await Apply(companion, memory.Companion);
		}
		else
		{
			Log.Error("Attempted to switch companion when character does not have a companion summoned");
		}
	}

	public static async Task SwitchOrnament(ActorMemory memory, Ornament ornament)
	{
		if (memory.IsUsingOrnament == true && memory.Ornament != null)
		{
			memory.OrnamentId = (ushort)ornament.RowId;
			memory.Ornament.AttachmentPoint = (byte)ornament.AttachPoint;
			await Apply(ornament, memory.Ornament);
		}
		else
		{
			Log.Error("Attempted to switch ornament when character does not have an ornament equipped");
		}
	}

	private static async Task Apply(INpcBase npc, ActorMemory targetActor)
	{
		try
		{
			CharacterFile apFile = npc.ToFile();
			await apFile.Apply(targetActor, CharacterFile.SaveModes.EquipmentGear);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to load appearance for {npc.TypeName} {npc.Name}");
		}
	}

	private static float CalculateMountScale(Mount mount, ActorMemory memory)
	{
		if (memory.Customize == null)
			return 1.0f;

		var mountCustomize = GameDataService.MountCustomize.Get(mount.MountCustomizeRow);
		int mountScaleFactor = 100;

		switch (memory.Customize.Race)
		{
			case ActorCustomizeMemory.Races.Hyur:
				switch (memory.Customize.Gender)
				{
					case ActorCustomizeMemory.Genders.Masculine:
						switch (memory.Customize.Tribe)
						{
							case ActorCustomizeMemory.Tribes.Midlander:
								mountScaleFactor = mountCustomize.HyurMidlanderMaleScale;
								break;
							case ActorCustomizeMemory.Tribes.Highlander:
								mountScaleFactor = mountCustomize.HyurHighlanderMaleScale;
								break;
						}

						break;

					case ActorCustomizeMemory.Genders.Feminine:
						switch (memory.Customize.Tribe)
						{
							case ActorCustomizeMemory.Tribes.Midlander:
								mountScaleFactor = mountCustomize.HyurMidlanderFemaleScale;
								break;
							case ActorCustomizeMemory.Tribes.Highlander:
								mountScaleFactor = mountCustomize.HyurHighlanderFemaleScale;
								break;
						}

						break;
				}

				break;

			case ActorCustomizeMemory.Races.Elezen:
				switch (memory.Customize.Gender)
				{
					case ActorCustomizeMemory.Genders.Masculine:
						mountScaleFactor = mountCustomize.ElezenMaleScale;
						break;

					case ActorCustomizeMemory.Genders.Feminine:
						mountScaleFactor = mountCustomize.ElezenFemaleScale;
						break;
				}

				break;

			case ActorCustomizeMemory.Races.Lalafel:
				switch (memory.Customize.Gender)
				{
					case ActorCustomizeMemory.Genders.Masculine:
						mountScaleFactor = mountCustomize.LalaMaleScale;
						break;

					case ActorCustomizeMemory.Genders.Feminine:
						mountScaleFactor = mountCustomize.LalaFemaleScale;
						break;
				}

				break;

			case ActorCustomizeMemory.Races.Miqote:
				switch (memory.Customize.Gender)
				{
					case ActorCustomizeMemory.Genders.Masculine:
						mountScaleFactor = mountCustomize.MiqoMaleScale;
						break;

					case ActorCustomizeMemory.Genders.Feminine:
						mountScaleFactor = mountCustomize.MiqoFemaleScale;
						break;
				}

				break;

			case ActorCustomizeMemory.Races.Roegadyn:
				switch (memory.Customize.Gender)
				{
					case ActorCustomizeMemory.Genders.Masculine:
						mountScaleFactor = mountCustomize.RoeMaleScale;
						break;

					case ActorCustomizeMemory.Genders.Feminine:
						mountScaleFactor = mountCustomize.RoeFemaleScale;
						break;
				}

				break;

			case ActorCustomizeMemory.Races.AuRa:
				switch (memory.Customize.Gender)
				{
					case ActorCustomizeMemory.Genders.Masculine:
						mountScaleFactor = mountCustomize.AuRaMaleScale;
						break;

					case ActorCustomizeMemory.Genders.Feminine:
						mountScaleFactor = mountCustomize.AuRaFemaleScale;
						break;
				}

				break;

			case ActorCustomizeMemory.Races.Hrothgar:
				switch (memory.Customize.Gender)
				{
					case ActorCustomizeMemory.Genders.Masculine:
						mountScaleFactor = mountCustomize.HrothgarMaleScale;
						break;
				}

				break;

			case ActorCustomizeMemory.Races.Viera:
				switch (memory.Customize.Gender)
				{
					case ActorCustomizeMemory.Genders.Masculine:
						mountScaleFactor = mountCustomize.VieraMaleScale;
						break;

					case ActorCustomizeMemory.Genders.Feminine:
						mountScaleFactor = mountCustomize.VieraFemaleScale;
						break;
				}

				break;
		}

		float realScale = mountScaleFactor / 100f;
		return realScale;
	}
}
