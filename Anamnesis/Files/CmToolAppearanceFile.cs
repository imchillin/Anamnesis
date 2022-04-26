// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.Globalization;
using Anamnesis.Memory;

#pragma warning disable IDE1006, SA1300
public class CmToolAppearanceFile : JsonFileBase, IUpgradeCharacterFile
{
	public override string FileExtension => ".cma";
	public override string TypeName => "CMTool Appearance";

	public Item? MainHand { get; set; }
	public Item? OffHand { get; set; }
	public string? EquipmentBytes { get; set; }
	public string? CharacterBytes { get; set; }
	public Details? characterDetails { get; set; }

	public CharacterFile Upgrade()
	{
		CharacterFile file = new CharacterFile();
		file.SaveMode = CharacterFile.SaveModes.All;
		file.MainHand = this.MainHand?.Upgrade();
		file.OffHand = this.OffHand?.Upgrade();

		if (this.EquipmentBytes != null)
		{
			byte[] data = this.StringtoBytes(this.EquipmentBytes);

			// From CM2: CharacterDetailsView2.xaml.cs line 801
			file.HeadGear = new CharacterFile.ItemSave();
			file.HeadGear.ModelBase = (ushort)(data[0] + (data[1] * 256));
			file.HeadGear.ModelVariant = data[2];
			file.HeadGear.DyeId = data[3];

			file.Body = new CharacterFile.ItemSave();
			file.Body.ModelBase = (ushort)(data[4] + (data[5] * 256));
			file.Body.ModelVariant = data[6];
			file.Body.DyeId = data[7];

			file.Hands = new CharacterFile.ItemSave();
			file.Hands.ModelBase = (ushort)(data[8] + (data[9] * 256));
			file.Hands.ModelVariant = data[10];
			file.Hands.DyeId = data[11];

			file.Legs = new CharacterFile.ItemSave();
			file.Legs.ModelBase = (ushort)(data[12] + (data[13] * 256));
			file.Legs.ModelVariant = data[14];
			file.Legs.DyeId = data[15];

			file.Feet = new CharacterFile.ItemSave();
			file.Feet.ModelBase = (ushort)(data[16] + (data[17] * 256));
			file.Feet.ModelVariant = data[18];
			file.Feet.DyeId = data[19];

			file.Ears = new CharacterFile.ItemSave();
			file.Ears.ModelBase = (ushort)(data[20] + (data[21] * 256));
			file.Ears.ModelVariant = data[22];

			file.Neck = new CharacterFile.ItemSave();
			file.Neck.ModelBase = (ushort)(data[24] + (data[25] * 256));
			file.Neck.ModelVariant = data[26];

			file.Wrists = new CharacterFile.ItemSave();
			file.Wrists.ModelBase = (ushort)(data[28] + (data[29] * 256));
			file.Wrists.ModelVariant = data[30];

			file.RightRing = new CharacterFile.ItemSave();
			file.RightRing.ModelBase = (ushort)(data[32] + (data[33] * 256));
			file.RightRing.ModelVariant = data[34];

			file.LeftRing = new CharacterFile.ItemSave();
			file.LeftRing.ModelBase = (ushort)(data[36] + (data[37] * 256));
			file.LeftRing.ModelVariant = data[38];
		}

		if (this.CharacterBytes != null)
		{
			byte[] data = this.StringtoBytes(this.CharacterBytes);

			// From CM2: MainWindow.xaml.cs line 708
			file.Race = (ActorCustomizeMemory.Races)data[0];
			file.Gender = (ActorCustomizeMemory.Genders)data[1];
			file.Age = (ActorCustomizeMemory.Ages)data[2];
			file.Height = data[3];
			file.Tribe = (ActorCustomizeMemory.Tribes)data[4];
			file.Head = data[5];
			file.Hair = data[6];
			file.EnableHighlights = data[7] != 0;
			file.Skintone = data[8];
			file.REyeColor = data[9];
			file.HairTone = data[10];
			file.Highlights = data[11];
			file.FacialFeatures = (ActorCustomizeMemory.FacialFeature)data[12];
			file.LimbalEyes = data[13];
			file.Eyebrows = data[14];
			file.LEyeColor = data[15];
			file.Eyes = data[16];
			file.Nose = data[17];
			file.Jaw = data[18];
			file.Mouth = data[19];
			file.LipsToneFurPattern = data[20];
			file.EarMuscleTailSize = data[21];
			file.TailEarsType = data[22];
			file.Bust = data[23];
			file.FacePaint = data[24];
			file.FacePaintColor = data[25];

			if (this.characterDetails?.ModelType != null)
				file.ModelType = (uint)this.characterDetails.ModelType.value;

			if (this.characterDetails?.SkinRedPigment != null && this.characterDetails.SkinGreenPigment != null && this.characterDetails.SkinBluePigment != null)
				file.SkinColor = new Color((float)this.characterDetails.SkinRedPigment.value, (float)this.characterDetails.SkinGreenPigment.value, (float)this.characterDetails.SkinBluePigment.value);

			if (this.characterDetails?.SkinRedGloss != null && this.characterDetails.SkinGreenGloss != null && this.characterDetails.SkinBlueGloss != null)
				file.SkinGloss = new Color((float)this.characterDetails.SkinRedGloss.value, (float)this.characterDetails.SkinGreenGloss.value, (float)this.characterDetails.SkinBlueGloss.value);

			if (this.characterDetails?.LeftEyeRed != null && this.characterDetails.LeftEyeGreen != null && this.characterDetails.LeftEyeBlue != null)
				file.LeftEyeColor = new Color((float)this.characterDetails.LeftEyeRed.value, (float)this.characterDetails.LeftEyeGreen.value, (float)this.characterDetails.LeftEyeBlue.value);

			if (this.characterDetails?.RightEyeRed != null && this.characterDetails.RightEyeBlue != null && this.characterDetails.RightEyeGreen != null)
				file.RightEyeColor = new Color((float)this.characterDetails.RightEyeRed.value, (float)this.characterDetails.RightEyeGreen.value, (float)this.characterDetails.RightEyeBlue.value);

			if (this.characterDetails?.LimbalR != null && this.characterDetails.LimbalG != null && this.characterDetails.LimbalB != null)
				file.LimbalRingColor = new Color((float)this.characterDetails.LimbalR.value, (float)this.characterDetails.LimbalG.value, (float)this.characterDetails.LimbalB.value);

			if (this.characterDetails?.HairRedPigment != null && this.characterDetails.HairGreenPigment != null && this.characterDetails.HairBluePigment != null)
				file.HairColor = new Color((float)this.characterDetails.HairRedPigment.value, (float)this.characterDetails.HairGreenPigment.value, (float)this.characterDetails.HairBluePigment.value);

			if (this.characterDetails?.HairGlowRed != null && this.characterDetails.HairGlowGreen != null && this.characterDetails.HairGlowBlue != null)
				file.HairGloss = new Color((float)this.characterDetails.HairGlowRed.value, (float)this.characterDetails.HairGlowGreen.value, (float)this.characterDetails.HairGlowBlue.value);

			if (this.characterDetails?.HighlightRedPigment != null && this.characterDetails.HighlightGreenPigment != null && this.characterDetails.HighlightBluePigment != null)
				file.HairHighlight = new Color((float)this.characterDetails.HighlightRedPigment.value, (float)this.characterDetails.HighlightGreenPigment.value, (float)this.characterDetails.HighlightBluePigment.value);

			if (this.characterDetails?.LipsR != null && this.characterDetails?.LipsG != null && this.characterDetails?.LipsB != null && this.characterDetails?.LipsBrightness != null)
				file.MouthColor = new Color4((float)this.characterDetails.LipsR.value, (float)this.characterDetails.LipsG.value, (float)this.characterDetails.LipsB.value, (float)this.characterDetails.LipsBrightness.value);

			if (this.characterDetails?.Height != null)
				file.HeightMultiplier = (float)this.characterDetails.Height.value;
		}

		return file;
	}

	protected byte[] StringtoBytes(string str)
	{
		string[] parts = str.Split(' ');
		byte[] data = new byte[parts.Length];
		for (int i = 0; i < parts.Length; i++)
		{
			data[i] = byte.Parse(parts[i], NumberStyles.HexNumber);
		}

		return data;
	}

	[Serializable]
	public class Item
	{
		// Set.
		public ushort Item1 { get; set; }

		// Base.
		public ushort Item2 { get; set; }

		// Variant.
		public ushort Item3 { get; set; }

		// Dye.
		public ushort Item4 { get; set; }

		public CharacterFile.WeaponSave Upgrade()
		{
			CharacterFile.WeaponSave item = new CharacterFile.WeaponSave();
			item.ModelSet = this.Item1;
			item.ModelBase = this.Item2;
			item.ModelVariant = (byte)this.Item3;
			item.DyeId = (byte)this.Item4;
			item.Scale = Vector.One;
			item.Color = Color.White;
			return item;
		}
	}

	public class Details
	{
		public Entry? TailSize { get; set; }
		public Entry? Height { get; set; }
		public Entry? BustX { get; set; }
		public Entry? BustY { get; set; }
		public Entry? BustZ { get; set; }
		public Entry? Voices { get; set; }
		public Entry? MuscleTone { get; set; }
		public Entry? WeaponX { get; set; }
		public Entry? WeaponY { get; set; }
		public Entry? WeaponZ { get; set; }
		public Entry? OffHandX { get; set; }
		public Entry? OffHandY { get; set; }
		public Entry? OffHandZ { get; set; }
		public Entry? OffHandRed { get; set; }
		public Entry? OffHandGreen { get; set; }
		public Entry? OffHandBlue { get; set; }
		public Entry? WeaponRed { get; set; }
		public Entry? WeaponGreen { get; set; }
		public Entry? WeaponBlue { get; set; }
		public Entry? SkinRedPigment { get; set; }
		public Entry? SkinGreenPigment { get; set; }
		public Entry? SkinBluePigment { get; set; }
		public Entry? SkinRedGloss { get; set; }
		public Entry? SkinGreenGloss { get; set; }
		public Entry? SkinBlueGloss { get; set; }
		public Entry? HairRedPigment { get; set; }
		public Entry? HairGreenPigment { get; set; }
		public Entry? HairBluePigment { get; set; }
		public Entry? HairGlowRed { get; set; }
		public Entry? HairGlowGreen { get; set; }
		public Entry? HairGlowBlue { get; set; }
		public Entry? HighlightRedPigment { get; set; }
		public Entry? HighlightGreenPigment { get; set; }
		public Entry? HighlightBluePigment { get; set; }
		public Entry? LeftEyeRed { get; set; }
		public Entry? LeftEyeGreen { get; set; }
		public Entry? LeftEyeBlue { get; set; }
		public Entry? RightEyeRed { get; set; }
		public Entry? RightEyeGreen { get; set; }
		public Entry? RightEyeBlue { get; set; }
		public Entry? LipsBrightness { get; set; }
		public Entry? LipsR { get; set; }
		public Entry? LipsG { get; set; }
		public Entry? LipsB { get; set; }
		public Entry? LimbalR { get; set; }
		public Entry? LimbalG { get; set; }
		public Entry? LimbalB { get; set; }
		public Entry? ModelType { get; set; }
	}

	public class Entry
	{
		public double value { get; set; }
	}
}
