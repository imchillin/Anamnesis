// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Files
{
	using ConceptMatrix;

	#pragma warning disable IDE1006, SA1300
	public class LegacyAllFile : LegacyEquipmentSetFile
	{
		public static readonly FileType AllFileType = new FileType("cma", "CM2 All Appearance File", typeof(LegacyAllFile));

		public string CharacterBytes { get; set; }
		public Details characterDetails { get; set; }

		public new FileBase Upgrade()
		{
			AllFile allFile = new AllFile();
			allFile.Equipment = base.Upgrade();
			allFile.Appearance = new AppearanceSetFile();

			byte[] data = this.StringtoBytes(this.CharacterBytes);

			// From CM2: MainWindow.xaml.cs line 708
			allFile.Appearance.Race = (Appearance.Races)data[0];
			allFile.Appearance.Gender = (Appearance.Genders)data[1];
			allFile.Appearance.Age = (Appearance.Ages)data[2];
			allFile.Appearance.Height = data[3];
			allFile.Appearance.Tribe = (Appearance.Tribes)data[4];
			allFile.Appearance.Head = data[5];
			allFile.Appearance.Hair = data[6];
			allFile.Appearance.Highlights = data[7];
			allFile.Appearance.Skintone = data[8];
			allFile.Appearance.REyeColor = data[9];
			allFile.Appearance.HairTone = data[10];
			allFile.Appearance.Highlights = data[11];
			allFile.Appearance.FacialFeatures = (Appearance.FacialFeature)data[12];
			allFile.Appearance.LimbalEyes = data[13];
			allFile.Appearance.Eyebrows = data[14];
			allFile.Appearance.LEyeColor = data[15];
			allFile.Appearance.Eyes = data[16];
			allFile.Appearance.Nose = data[17];
			allFile.Appearance.Jaw = data[18];
			allFile.Appearance.Lips = data[19];
			allFile.Appearance.Lips = data[20];
			allFile.Appearance.EarMuscleTailSize = data[21];
			allFile.Appearance.TailEarsType = data[22];
			allFile.Appearance.Bust = data[23];
			allFile.Appearance.FacePaint = data[24];
			allFile.Appearance.FacePaintColor = data[25];

			// If this file has no actor data, treat it as an equipment file
			if (allFile.Appearance.Race == 0)
				return allFile.Equipment;

			return allFile;
		}

		public override FileType GetFileType()
		{
			return AllFileType;
		}

		public class Details
		{
			public Entry TailSize { get; set; }
			public Entry Height { get; set; }
			public Entry BustX { get; set; }
			public Entry BustY { get; set; }
			public Entry BustZ { get; set; }
			public Entry Voices { get; set; }
			public Entry MuscleTone { get; set; }
			public Entry WeaponX { get; set; }
			public Entry WeaponY { get; set; }
			public Entry WeaponZ { get; set; }
			public Entry OffHandX { get; set; }
			public Entry OffHandY { get; set; }
			public Entry OffHandZ { get; set; }
			public Entry OffHandRed { get; set; }
			public Entry OffHandGreen { get; set; }
			public Entry OffHandBlue { get; set; }
			public Entry WeaponRed { get; set; }
			public Entry WeaponGreen { get; set; }
			public Entry WeaponBlue { get; set; }
			public Entry SkinRedPigment { get; set; }
			public Entry SkinGreenPigment { get; set; }
			public Entry SkinBluePigment { get; set; }
			public Entry SkinRedGloss { get; set; }
			public Entry SkinGreenGloss { get; set; }
			public Entry SkinBlueGloss { get; set; }
			public Entry HairRedPigment { get; set; }
			public Entry HairGreenPigment { get; set; }
			public Entry HairBluePigment { get; set; }
			public Entry HairGlowRed { get; set; }
			public Entry HairGlowGreen { get; set; }
			public Entry HairGlowBlue { get; set; }
			public Entry HighlightRedPigment { get; set; }
			public Entry HighlightGreenPigment { get; set; }
			public Entry HighhlightBluePigment { get; set; }
			public Entry LeftEyeRed { get; set; }
			public Entry LeftEyeGreen { get; set; }
			public Entry LeftEyeBlue { get; set; }
			public Entry RightEyeRed { get; set; }
			public Entry RightEyeGreen { get; set; }
			public Entry RightEyeBlue { get; set; }
			public Entry LipsBrightness { get; set; }
			public Entry LipsR { get; set; }
			public Entry LipsG { get; set; }
			public Entry LipsB { get; set; }
			public Entry LimbalR { get; set; }
			public Entry LimbalG { get; set; }
			public Entry LimbalB { get; set; }
		}

		public class Entry
		{
			public double value { get; set; }
		}
	}
}
