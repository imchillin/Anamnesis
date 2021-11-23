// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.IO;
	using Anamnesis.Memory;
	using Anamnesis.Serialization;
	using static Anamnesis.Memory.ActorCustomizeMemory;

	public class LegacyLegacyCharacterFileJson : JsonFileBase, IUpgradeCharacterFile
	{
		public override string FileExtension => ".json";

		public LegacyCharacterFile.Entry? TailSize { get; set; }
		public LegacyCharacterFile.Entry? Race { get; set; }
		public LegacyCharacterFile.Entry? Clan { get; set; }
		public LegacyCharacterFile.Entry? Gender { get; set; }
		public LegacyCharacterFile.Entry? Height { get; set; }
		public LegacyCharacterFile.Entry? BustX { get; set; }
		public LegacyCharacterFile.Entry? BustY { get; set; }
		public LegacyCharacterFile.Entry? BustZ { get; set; }
		public LegacyCharacterFile.Entry? Head { get; set; }
		public LegacyCharacterFile.Entry? Hair { get; set; }
		public LegacyCharacterFile.Entry? TailType { get; set; }
		public LegacyCharacterFile.Entry? Jaw { get; set; }
		public LegacyCharacterFile.Entry? RHeight { get; set; }
		public LegacyCharacterFile.Entry? RBust { get; set; }
		public LegacyCharacterFile.Entry? HairTone { get; set; }
		public LegacyCharacterFile.Entry? Highlights { get; set; }
		public LegacyCharacterFile.Entry? HighlightTone { get; set; }
		public LegacyCharacterFile.Entry? Skintone { get; set; }
		public LegacyCharacterFile.Entry? FacialFeatures { get; set; }
		public LegacyCharacterFile.Entry? Eye { get; set; }
		public LegacyCharacterFile.Entry? RightEye { get; set; }
		public LegacyCharacterFile.Entry? LeftEye { get; set; }
		public LegacyCharacterFile.Entry? FacePaint { get; set; }
		public LegacyCharacterFile.Entry? FacePaintColor { get; set; }
		public LegacyCharacterFile.Entry? Nose { get; set; }
		public LegacyCharacterFile.Entry? Lips { get; set; }
		public LegacyCharacterFile.Entry? LipsTone { get; set; }
		public LegacyCharacterFile.Entry? EyeBrowType { get; set; }
		public LegacyCharacterFile.Entry? Voices { get; set; }
		public LegacyCharacterFile.Entry? TailorMuscle { get; set; }
		public LegacyCharacterFile.Entry? MuscleTone { get; set; }
		public LegacyCharacterFile.Entry? Job { get; set; }
		public LegacyCharacterFile.Entry? WeaponBase { get; set; }
		public LegacyCharacterFile.Entry? WeaponV { get; set; }
		public LegacyCharacterFile.Entry? WeaponDye { get; set; }
		public LegacyCharacterFile.Entry? WeaponX { get; set; }
		public LegacyCharacterFile.Entry? WeaponY { get; set; }
		public LegacyCharacterFile.Entry? WeaponZ { get; set; }
		public LegacyCharacterFile.Entry? HeadPiece { get; set; }
		public LegacyCharacterFile.Entry? HeadV { get; set; }
		public LegacyCharacterFile.Entry? HeadDye { get; set; }
		public LegacyCharacterFile.Entry? Chest { get; set; }
		public LegacyCharacterFile.Entry? ChestV { get; set; }
		public LegacyCharacterFile.Entry? ChestDye { get; set; }
		public LegacyCharacterFile.Entry? Arms { get; set; }
		public LegacyCharacterFile.Entry? ArmsV { get; set; }
		public LegacyCharacterFile.Entry? ArmsDye { get; set; }
		public LegacyCharacterFile.Entry? Legs { get; set; }
		public LegacyCharacterFile.Entry? LegsV { get; set; }
		public LegacyCharacterFile.Entry? LegsDye { get; set; }
		public LegacyCharacterFile.Entry? Feet { get; set; }
		public LegacyCharacterFile.Entry? FeetVa { get; set; }
		public LegacyCharacterFile.Entry? FeetDye { get; set; }
		public LegacyCharacterFile.Entry? Ear { get; set; }
		public LegacyCharacterFile.Entry? EarVa { get; set; }
		public LegacyCharacterFile.Entry? Neck { get; set; }
		public LegacyCharacterFile.Entry? NeckVa { get; set; }
		public LegacyCharacterFile.Entry? Wrist { get; set; }
		public LegacyCharacterFile.Entry? WristVa { get; set; }
		public LegacyCharacterFile.Entry? RFinger { get; set; }
		public LegacyCharacterFile.Entry? RFingerVa { get; set; }
		public LegacyCharacterFile.Entry? LFinger { get; set; }
		public LegacyCharacterFile.Entry? Offhand { get; set; }
		public LegacyCharacterFile.Entry? OffhandBase { get; set; }
		public LegacyCharacterFile.Entry? OffhandV { get; set; }
		public LegacyCharacterFile.Entry? OffhandDye { get; set; }
		public LegacyCharacterFile.Entry? OffhandX { get; set; }
		public LegacyCharacterFile.Entry? OffhandY { get; set; }
		public LegacyCharacterFile.Entry? OffhandZ { get; set; }
		public LegacyCharacterFile.Entry? OffhandRed { get; set; }
		public LegacyCharacterFile.Entry? OffhandGreen { get; set; }
		public LegacyCharacterFile.Entry? OffhandBlue { get; set; }
		public LegacyCharacterFile.Entry? LFingerVa { get; set; }
		public LegacyCharacterFile.Entry? WeaponRed { get; set; }
		public LegacyCharacterFile.Entry? WeaponGreen { get; set; }
		public LegacyCharacterFile.Entry? WeaponBlue { get; set; }
		public LegacyCharacterFile.Entry? SkinRedPigment { get; set; }
		public LegacyCharacterFile.Entry? SkinGreenPigment { get; set; }
		public LegacyCharacterFile.Entry? SkinBluePigment { get; set; }
		public LegacyCharacterFile.Entry? SkinRedGloss { get; set; }
		public LegacyCharacterFile.Entry? SkinGreenGloss { get; set; }
		public LegacyCharacterFile.Entry? SkinBlueGloss { get; set; }
		public LegacyCharacterFile.Entry? HairRedPigment { get; set; }
		public LegacyCharacterFile.Entry? HairGreenPigment { get; set; }
		public LegacyCharacterFile.Entry? HairBluePigment { get; set; }
		public LegacyCharacterFile.Entry? HairGlowRed { get; set; }
		public LegacyCharacterFile.Entry? HairGlowGreen { get; set; }
		public LegacyCharacterFile.Entry? HairGlowBlue { get; set; }
		public LegacyCharacterFile.Entry? HighlightRedPigment { get; set; }
		public LegacyCharacterFile.Entry? HighlightGreenPigment { get; set; }
		public LegacyCharacterFile.Entry? HighlightBluePigment { get; set; }
		public LegacyCharacterFile.Entry? LeftEyeRed { get; set; }
		public LegacyCharacterFile.Entry? LeftEyeGreen { get; set; }
		public LegacyCharacterFile.Entry? LeftEyeBlue { get; set; }
		public LegacyCharacterFile.Entry? RightEyeRed { get; set; }
		public LegacyCharacterFile.Entry? RightEyeGreen { get; set; }
		public LegacyCharacterFile.Entry? RightEyeBlue { get; set; }
		public LegacyCharacterFile.Entry? LipsBrightness { get; set; }
		public LegacyCharacterFile.Entry? LipsR { get; set; }
		public LegacyCharacterFile.Entry? LipsB { get; set; }
		public LegacyCharacterFile.Entry? LipsG { get; set; }
		public LegacyCharacterFile.Entry? LimbalR { get; set; }
		public LegacyCharacterFile.Entry? LimbalG { get; set; }
		public LegacyCharacterFile.Entry? LimbalB { get; set; }
		public LegacyCharacterFile.Entry? LimbalEyes { get; set; }
		public LegacyCharacterFile.Entry? BodyType { get; set; }

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

		private CharacterFile.ItemSave? GetItem(LegacyCharacterFile.Entry? baseVal, LegacyCharacterFile.Entry? variant, LegacyCharacterFile.Entry? dye = null)
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

		private Color? GetColor(LegacyCharacterFile.Entry? r, LegacyCharacterFile.Entry? g, LegacyCharacterFile.Entry? b)
		{
			if (r == null || g == null || b == null)
				return null;

			Color c = default;
			c.R = (float)r.value;
			c.G = (float)g.value;
			c.B = (float)b.value;

			return c;
		}

		private Color4? GetColor4(LegacyCharacterFile.Entry? r, LegacyCharacterFile.Entry? g, LegacyCharacterFile.Entry? b, LegacyCharacterFile.Entry? a)
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

		private Vector? GetVector(LegacyCharacterFile.Entry? x, LegacyCharacterFile.Entry? y, LegacyCharacterFile.Entry? z)
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
}
