// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;
using Anamnesis.Memory;
using Anamnesis.Serialization;
using static Anamnesis.Memory.ActorCustomizeMemory;

public class CmToolLegacyAppearanceFile : JsonFileBase, IUpgradeCharacterFile
{
	public override string FileExtension => ".json";

	public CmToolAppearanceFile.Entry? TailSize { get; set; }
	public CmToolAppearanceFile.Entry? Race { get; set; }
	public CmToolAppearanceFile.Entry? Clan { get; set; }
	public CmToolAppearanceFile.Entry? Gender { get; set; }
	public CmToolAppearanceFile.Entry? Height { get; set; }
	public CmToolAppearanceFile.Entry? BustX { get; set; }
	public CmToolAppearanceFile.Entry? BustY { get; set; }
	public CmToolAppearanceFile.Entry? BustZ { get; set; }
	public CmToolAppearanceFile.Entry? Head { get; set; }
	public CmToolAppearanceFile.Entry? Hair { get; set; }
	public CmToolAppearanceFile.Entry? TailType { get; set; }
	public CmToolAppearanceFile.Entry? Jaw { get; set; }
	public CmToolAppearanceFile.Entry? RHeight { get; set; }
	public CmToolAppearanceFile.Entry? RBust { get; set; }
	public CmToolAppearanceFile.Entry? HairTone { get; set; }
	public CmToolAppearanceFile.Entry? Highlights { get; set; }
	public CmToolAppearanceFile.Entry? HighlightTone { get; set; }
	public CmToolAppearanceFile.Entry? Skintone { get; set; }
	public CmToolAppearanceFile.Entry? FacialFeatures { get; set; }
	public CmToolAppearanceFile.Entry? Eye { get; set; }
	public CmToolAppearanceFile.Entry? RightEye { get; set; }
	public CmToolAppearanceFile.Entry? LeftEye { get; set; }
	public CmToolAppearanceFile.Entry? FacePaint { get; set; }
	public CmToolAppearanceFile.Entry? FacePaintColor { get; set; }
	public CmToolAppearanceFile.Entry? Nose { get; set; }
	public CmToolAppearanceFile.Entry? Lips { get; set; }
	public CmToolAppearanceFile.Entry? LipsTone { get; set; }
	public CmToolAppearanceFile.Entry? EyeBrowType { get; set; }
	public CmToolAppearanceFile.Entry? Voices { get; set; }
	public CmToolAppearanceFile.Entry? TailorMuscle { get; set; }
	public CmToolAppearanceFile.Entry? MuscleTone { get; set; }
	public CmToolAppearanceFile.Entry? Job { get; set; }
	public CmToolAppearanceFile.Entry? WeaponBase { get; set; }
	public CmToolAppearanceFile.Entry? WeaponV { get; set; }
	public CmToolAppearanceFile.Entry? WeaponDye { get; set; }
	public CmToolAppearanceFile.Entry? WeaponX { get; set; }
	public CmToolAppearanceFile.Entry? WeaponY { get; set; }
	public CmToolAppearanceFile.Entry? WeaponZ { get; set; }
	public CmToolAppearanceFile.Entry? HeadPiece { get; set; }
	public CmToolAppearanceFile.Entry? HeadV { get; set; }
	public CmToolAppearanceFile.Entry? HeadDye { get; set; }
	public CmToolAppearanceFile.Entry? Chest { get; set; }
	public CmToolAppearanceFile.Entry? ChestV { get; set; }
	public CmToolAppearanceFile.Entry? ChestDye { get; set; }
	public CmToolAppearanceFile.Entry? Arms { get; set; }
	public CmToolAppearanceFile.Entry? ArmsV { get; set; }
	public CmToolAppearanceFile.Entry? ArmsDye { get; set; }
	public CmToolAppearanceFile.Entry? Legs { get; set; }
	public CmToolAppearanceFile.Entry? LegsV { get; set; }
	public CmToolAppearanceFile.Entry? LegsDye { get; set; }
	public CmToolAppearanceFile.Entry? Feet { get; set; }
	public CmToolAppearanceFile.Entry? FeetVa { get; set; }
	public CmToolAppearanceFile.Entry? FeetDye { get; set; }
	public CmToolAppearanceFile.Entry? Ear { get; set; }
	public CmToolAppearanceFile.Entry? EarVa { get; set; }
	public CmToolAppearanceFile.Entry? Neck { get; set; }
	public CmToolAppearanceFile.Entry? NeckVa { get; set; }
	public CmToolAppearanceFile.Entry? Wrist { get; set; }
	public CmToolAppearanceFile.Entry? WristVa { get; set; }
	public CmToolAppearanceFile.Entry? RFinger { get; set; }
	public CmToolAppearanceFile.Entry? RFingerVa { get; set; }
	public CmToolAppearanceFile.Entry? LFinger { get; set; }
	public CmToolAppearanceFile.Entry? Offhand { get; set; }
	public CmToolAppearanceFile.Entry? OffhandBase { get; set; }
	public CmToolAppearanceFile.Entry? OffhandV { get; set; }
	public CmToolAppearanceFile.Entry? OffhandDye { get; set; }
	public CmToolAppearanceFile.Entry? OffhandX { get; set; }
	public CmToolAppearanceFile.Entry? OffhandY { get; set; }
	public CmToolAppearanceFile.Entry? OffhandZ { get; set; }
	public CmToolAppearanceFile.Entry? OffhandRed { get; set; }
	public CmToolAppearanceFile.Entry? OffhandGreen { get; set; }
	public CmToolAppearanceFile.Entry? OffhandBlue { get; set; }
	public CmToolAppearanceFile.Entry? LFingerVa { get; set; }
	public CmToolAppearanceFile.Entry? WeaponRed { get; set; }
	public CmToolAppearanceFile.Entry? WeaponGreen { get; set; }
	public CmToolAppearanceFile.Entry? WeaponBlue { get; set; }
	public CmToolAppearanceFile.Entry? SkinRedPigment { get; set; }
	public CmToolAppearanceFile.Entry? SkinGreenPigment { get; set; }
	public CmToolAppearanceFile.Entry? SkinBluePigment { get; set; }
	public CmToolAppearanceFile.Entry? SkinRedGloss { get; set; }
	public CmToolAppearanceFile.Entry? SkinGreenGloss { get; set; }
	public CmToolAppearanceFile.Entry? SkinBlueGloss { get; set; }
	public CmToolAppearanceFile.Entry? HairRedPigment { get; set; }
	public CmToolAppearanceFile.Entry? HairGreenPigment { get; set; }
	public CmToolAppearanceFile.Entry? HairBluePigment { get; set; }
	public CmToolAppearanceFile.Entry? HairGlowRed { get; set; }
	public CmToolAppearanceFile.Entry? HairGlowGreen { get; set; }
	public CmToolAppearanceFile.Entry? HairGlowBlue { get; set; }
	public CmToolAppearanceFile.Entry? HighlightRedPigment { get; set; }
	public CmToolAppearanceFile.Entry? HighlightGreenPigment { get; set; }
	public CmToolAppearanceFile.Entry? HighlightBluePigment { get; set; }
	public CmToolAppearanceFile.Entry? LeftEyeRed { get; set; }
	public CmToolAppearanceFile.Entry? LeftEyeGreen { get; set; }
	public CmToolAppearanceFile.Entry? LeftEyeBlue { get; set; }
	public CmToolAppearanceFile.Entry? RightEyeRed { get; set; }
	public CmToolAppearanceFile.Entry? RightEyeGreen { get; set; }
	public CmToolAppearanceFile.Entry? RightEyeBlue { get; set; }
	public CmToolAppearanceFile.Entry? LipsBrightness { get; set; }
	public CmToolAppearanceFile.Entry? LipsR { get; set; }
	public CmToolAppearanceFile.Entry? LipsB { get; set; }
	public CmToolAppearanceFile.Entry? LipsG { get; set; }
	public CmToolAppearanceFile.Entry? LimbalR { get; set; }
	public CmToolAppearanceFile.Entry? LimbalG { get; set; }
	public CmToolAppearanceFile.Entry? LimbalB { get; set; }
	public CmToolAppearanceFile.Entry? LimbalEyes { get; set; }
	public CmToolAppearanceFile.Entry? BodyType { get; set; }

	public CharacterFile Upgrade()
	{
		CharacterFile file = new CharacterFile();
		file.SaveMode = CharacterFile.SaveModes.All;

		file.Race = (Races?)this.Race?.value;
		file.Gender = (Genders?)this.Gender?.value;
		file.Age = (Ages?)this.BodyType?.value;
		file.Height = (byte?)this.Height?.value;
		file.Tribe = (Tribes?)this.Clan?.value;
		file.Head = (byte?)this.Head?.value;
		file.Hair = (byte?)this.Hair?.value;
		file.EnableHighlights = this.Highlights?.value > 0;
		file.Skintone = (byte?)this.Skintone?.value;
		file.REyeColor = (byte?)this.RightEye?.value;
		file.HairTone = (byte?)this.HairTone?.value;
		file.Highlights = (byte?)this.Highlights?.value;
		file.FacialFeatures = (FacialFeature?)this.FacialFeatures?.value;
		file.LimbalEyes = (byte?)this.LimbalEyes?.value;
		file.Eyebrows = (byte?)this.EyeBrowType?.value;
		file.LEyeColor = (byte?)this.LeftEye?.value;
		file.Eyes = (byte?)this.Eye?.value;
		file.Nose = (byte?)this.Nose?.value;
		file.Jaw = (byte?)this.Jaw?.value;
		file.Mouth = (byte?)this.Lips?.value;
		file.LipsToneFurPattern = (byte?)this.LipsTone?.value;
		file.EarMuscleTailSize = (byte?)this.TailSize?.value;
		file.TailEarsType = (byte?)this.TailType?.value;
		file.Bust = (byte?)this.RBust?.value;
		file.FacePaint = (byte?)this.FacePaint?.value;
		file.FacePaintColor = (byte?)this.FacePaintColor?.value;

		// weapons
		if (this.WeaponBase != null && this.WeaponV != null && this.Job != null)
		{
			file.MainHand = new CharacterFile.WeaponSave();
			file.MainHand.ModelSet = (ushort)this.Job.value;
			file.MainHand.ModelBase = (ushort)this.WeaponBase.value;
			file.MainHand.ModelVariant = (ushort)this.WeaponV.value;

			if (this.WeaponDye != null)
				file.MainHand.DyeId = (byte)this.WeaponDye.value;

			file.MainHand.Color = this.GetColor(this.WeaponRed, this.WeaponGreen, this.WeaponBlue) ?? Color.White;
		}

		if (this.OffhandBase != null && this.OffhandV != null && this.Offhand != null)
		{
			file.OffHand = new CharacterFile.WeaponSave();
			file.OffHand.ModelSet = (ushort)this.Offhand.value;
			file.OffHand.ModelBase = (ushort)this.OffhandBase.value;
			file.OffHand.ModelVariant = (ushort)this.OffhandV.value;

			if (this.OffhandDye != null)
				file.OffHand.DyeId = (byte)this.OffhandDye.value;

			file.OffHand.Color = this.GetColor(this.OffhandRed, this.OffhandGreen, this.OffhandBlue) ?? Color.White;
		}

		// equipment
		file.HeadGear = this.GetItem(this.HeadPiece, this.HeadV, this.HeadDye);
		file.Body = this.GetItem(this.Chest, this.ChestV, this.ChestDye);
		file.Hands = this.GetItem(this.Arms, this.ArmsV, this.ArmsDye);
		file.Legs = this.GetItem(this.Legs, this.LegsV, this.LegsDye);
		file.Feet = this.GetItem(this.Feet, this.FeetVa, this.FeetDye);
		file.Ears = this.GetItem(this.Ear, this.EarVa);
		file.Neck = this.GetItem(this.Neck, this.NeckVa);
		file.Wrists = this.GetItem(this.Wrist, this.WristVa);
		file.LeftRing = this.GetItem(this.LFinger, this.LFingerVa);
		file.RightRing = this.GetItem(this.RFinger, this.RFingerVa);

		// extended appearance
		// NOTE: extended weapon values are stored in the WeaponSave
		file.SkinColor = this.GetColor(this.SkinRedPigment, this.SkinGreenPigment, this.SkinBluePigment);
		file.SkinGloss = this.GetColor(this.SkinRedGloss, this.SkinGreenGloss, this.SkinBlueGloss);
		file.LeftEyeColor = this.GetColor(this.LeftEyeRed, this.LeftEyeGreen, this.LeftEyeBlue);
		file.RightEyeColor = this.GetColor(this.RightEyeRed, this.RightEyeGreen, this.RightEyeBlue);
		file.LimbalRingColor = this.GetColor(this.LimbalR, this.LimbalG, this.LimbalB);
		file.HairColor = this.GetColor(this.HairRedPigment, this.HairGreenPigment, this.HairBluePigment);
		////file.HairGloss = this.GetColor(this.HairGlowRed, this.HairGlowGreen, this.HairGlowRed);
		file.HairHighlight = this.GetColor(this.HairGlowRed, this.HairGlowGreen, this.HairGlowRed);
		file.MouthColor = this.GetColor4(this.LipsR, this.LipsG, this.LipsB, this.LipsBrightness);
		file.BustScale = this.GetVector(this.BustX, this.BustY, this.BustZ);
		////file.Transparency =
		file.MuscleTone = (float?)this.MuscleTone?.value;
		file.HeightMultiplier = (float?)this.Height?.value;

		return file;
	}

	public override FileBase Deserialize(Stream stream)
	{
		using TextReader reader = new StreamReader(stream);
		string json = reader.ReadToEnd();

		// Check for thse properties so that we dont try to load the newer json format (LegacyCharacterFile)
		// with this class by accident.
		if (json.Contains("characterDetails") || json.Contains("CharacterBytes"))
			throw new Exception("Invalid CM Json legacy legacy character file");

		return (FileBase)SerializerService.Deserialize(json, this.GetType());
	}

	private CharacterFile.ItemSave? GetItem(CmToolAppearanceFile.Entry? baseVal, CmToolAppearanceFile.Entry? variant, CmToolAppearanceFile.Entry? dye = null)
	{
		if (baseVal == null || variant == null)
			return null;

		CharacterFile.ItemSave save = new CharacterFile.ItemSave();
		save.ModelBase = (ushort)baseVal.value;
		save.ModelVariant = (byte)variant.value;

		if (dye != null)
			save.DyeId = (byte)dye.value;

		return save;
	}

	private Color? GetColor(CmToolAppearanceFile.Entry? r, CmToolAppearanceFile.Entry? g, CmToolAppearanceFile.Entry? b)
	{
		if (r == null || g == null || b == null)
			return null;

		Color c = default;
		c.R = (float)r.value;
		c.G = (float)g.value;
		c.B = (float)b.value;

		return c;
	}

	private Color4? GetColor4(CmToolAppearanceFile.Entry? r, CmToolAppearanceFile.Entry? g, CmToolAppearanceFile.Entry? b, CmToolAppearanceFile.Entry? a)
	{
		if (r == null || g == null || b == null || a == null)
			return null;

		Color4 c = default;
		c.R = (float)r.value;
		c.G = (float)g.value;
		c.B = (float)b.value;
		c.A = (float)a.value;

		return c;
	}

	private Vector? GetVector(CmToolAppearanceFile.Entry? x, CmToolAppearanceFile.Entry? y, CmToolAppearanceFile.Entry? z)
	{
		if (x == null || y == null || z == null)
			return null;

		Vector v = default;
		v.X = (float)x.value;
		v.Y = (float)y.value;
		v.Z = (float)z.value;

		return v;
	}
}
