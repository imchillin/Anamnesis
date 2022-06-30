// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Utilities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Anamnesis.Dialogs;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Utils;
using Lumina.Excel;

public static class NpcAppearanceSearch
{
	public static void Search(ActorMemory actor)
	{
		Task.Factory.StartNew(() => SearchAsync(actor));
	}

	public static async Task SearchAsync(ActorMemory actor)
	{
		List<INpcBase> appearances = new();

		using (WaitDialog dlg = await WaitDialog.ShowAsync("Please Wait...", "NPC Appearance search"))
		{
			await dlg.SetProgress(0.1);
			Search(actor, GameDataService.BattleNPCs, ref appearances);
			await dlg.SetProgress(0.5);
			Search(actor, GameDataService.EventNPCs, ref appearances);
			////await dlg.SetProgress(0.8);
			////Search(actor, GameDataService.ResidentNPCs, ref appearances);

			await dlg.SetProgress(1.0);
		}

		if (appearances.Count <= 0)
		{
			GenericDialog.Show($"Appearance not found", "NPC Appearance search");
		}
		else
		{
			StringBuilder messageBuilder = new();
			messageBuilder.AppendLine($"Found {appearances.Count} appearances:");
			messageBuilder.AppendLine();

			StringBuilder jsonBuilder = new();
			foreach (INpcBase appearance in appearances)
			{
				if (appearance is BattleNpc)
				{
					jsonBuilder.Append("B:");
				}
				else if (appearance is EventNpc)
				{
					jsonBuilder.Append("E:");
				}
				else if (appearance is ResidentNpc)
				{
					jsonBuilder.Append("R:");
				}

				jsonBuilder.AppendLine(appearance.RowId.ToString("D7"));
			}

			messageBuilder.AppendLine(jsonBuilder.ToString());
			messageBuilder.AppendLine();
			messageBuilder.AppendLine("This has been copied to your clipboard.");

			await ClipboardUtility.CopyToClipboardAsync(jsonBuilder.ToString());

			await GenericDialog.ShowAsync(messageBuilder.ToString(), "NPC Appearance search", MessageBoxButton.OK);
		}
	}

	private static void SearchNames(string name, ref List<ExcelRow> results)
	{
		foreach (BattleNpcName battleNpcName in GameDataService.BattleNpcNames)
		{
			if (battleNpcName.Name.ToLower() == name.ToLower())
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
