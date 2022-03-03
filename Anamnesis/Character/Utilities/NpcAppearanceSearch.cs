// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Utilities
{
	using System.Collections.Generic;
	using System.Text;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Excel;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Lumina.Excel;
	using Serilog;

	public static class NpcAppearanceSearch
	{
		public static void Search(ActorMemory actor)
		{
			List<ExcelRow> names = new();
			SearchNames(actor.Name, ref names);

			List<INpcBase> appearances = new();
			Search(actor, GameDataService.BattleNPCs, ref appearances);
			Search(actor, GameDataService.EventNPCs, ref appearances);
			Search(actor, GameDataService.ResidentNPCs, ref appearances);

			if (appearances.Count <= 0 && names.Count <= 0)
			{
				GenericDialog.Show("No results", "NPC Appearance search");
			}
			else
			{
				StringBuilder builder = new();
				if (appearances.Count > 0)
				{
					builder.Append(appearances.Count);
					builder.AppendLine(" appearances:");

					foreach (var result in appearances)
					{
						if (result is BattleNpc)
						{
							builder.Append("B:");
						}
						else if (result is EventNpc)
						{
							builder.Append("E:");
						}
						else if (result is ResidentNpc)
						{
							builder.Append("R:");
						}

						builder.Append(result.RowId.ToString());

						if (result.HasName)
						{
							builder.Append(" (");
							builder.Append(result.Name);
							builder.Append(")");
						}

						builder.AppendLine();
					}
				}

				if (names.Count > 0)
				{
					builder.Append(names.Count);
					builder.AppendLine(" names:");
					foreach (ExcelRow result in names)
					{
						if (result is BattleNpcName)
						{
							builder.Append("B:");
						}
						else if (result is EventNpc)
						{
							builder.Append("E:");
						}
						else if (result is ResidentNpc)
						{
							builder.Append("R:");
						}

						builder.AppendLine(result.RowId.ToString());
					}

					builder.AppendLine();
				}

				GenericDialog.Show(builder.ToString(), "NPC Appearance search");
			}
		}

		private static void SearchNames(string name, ref List<ExcelRow> results)
		{
			foreach(BattleNpcName battleNpcName in GameDataService.BattleNpcNames)
			{
				if (battleNpcName.Singular.ToLower() == name.ToLower())
				{
					results.Add(battleNpcName);
				}
			}

			foreach (ResidentNpc residentNpc in GameDataService.ResidentNPCs)
			{
				if (residentNpc.Name.ToLower() == name.ToLower())
				{
					results.Add(residentNpc);
				}
			}

			foreach (EventNpc eventNpc in GameDataService.EventNPCs)
			{
				if (eventNpc.Name.ToLower() == name.ToLower())
				{
					results.Add(eventNpc);
				}
			}
		}

		private static void Search(ActorMemory actor, IEnumerable<INpcBase> npcs, ref List<INpcBase> results)
		{
			foreach (INpcBase npc in npcs)
			{
				INpcAppearance? appearance = npc.GetAppearance();

				if (appearance == null)
					continue;

				if (IsAppearance(actor, appearance))
				{
					results.Add(npc);
				}
			}
		}

		private static bool IsAppearance(ActorMemory actor, INpcAppearance appearance)
		{
			if (actor.ModelType != appearance.ModelCharaRow)
				return false;

			if (actor.Customize != null)
			{
				if (!IsCustomize(actor.Customize, appearance))
				{
					return false;
				}
			}

			if (actor.Equipment != null)
			{
				if (!IsEquipment(actor.Equipment, appearance))
				{
					return false;
				}
			}

			return true;
		}

		// We dont check everything here, just enough to get unique.
		private static bool IsCustomize(ActorCustomizeMemory customize, INpcAppearance appearance)
		{
			// It gets weird checking models other than players for race and tribe, so... try and skip
			if (appearance.ModelCharaRow == 0)
			{
				if (customize.Race != appearance.Race?.CustomizeRace ||
					customize.Tribe != appearance.Tribe?.CustomizeTribe ||
					customize.Gender != (ActorCustomizeMemory.Genders)appearance.Gender ||
					customize.Age != (ActorCustomizeMemory.Ages)appearance.BodyType)
				{
					return false;
				}
			}

			if (customize.Skintone != appearance.SkinColor ||
				customize.Eyes != appearance.EyeShape ||
				customize.Head != appearance.Face ||
				(int)customize.FacialFeatures != appearance.FacialFeature ||
				appearance.FacePaint != appearance.FacePaint ||
				appearance.HairStyle != appearance.HairStyle ||
				customize.Mouth != appearance.Mouth ||
				customize.Height != appearance.Height)
			{
				return false;
			}

			return true;
		}

		private static bool IsEquipment(ActorEquipmentMemory equipment, INpcAppearance appearance)
		{
			if (equipment.Head?.Is(appearance.Head) == false)
				return false;

			if (equipment.Chest?.Is(appearance.Body) == false)
				return false;

			if (equipment.Legs?.Is(appearance.Legs) == false)
				return false;

			if (equipment.Feet?.Is(appearance.Feet) == false)
				return false;

			if (equipment.Arms?.Is(appearance.Hands) == false)
				return false;

			if (equipment.LFinger?.Is(appearance.LeftRing) == false)
				return false;

			if (equipment.RFinger?.Is(appearance.RightRing) == false)
				return false;

			if (equipment.Wrist?.Is(appearance.Wrists) == false)
				return false;

			if (equipment.Neck?.Is(appearance.Neck) == false)
				return false;

			if (equipment.Ear?.Is(appearance.Ears) == false)
				return false;

			return true;
		}
	}
}
