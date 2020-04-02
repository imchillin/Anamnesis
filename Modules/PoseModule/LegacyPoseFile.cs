// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Services;

	public class LegacyPoseFile : FileBase
	{
		public static readonly FileType FileType = new FileType(@"cmp", "CM2 Pose File", typeof(LegacyPoseFile));

		public string Description { get; set; }
		public string DateCreated { get; set; }
		public string CMPVersion { get; set; }
		public string Race { get; set; }
		public string Clan { get; set; }
		public string Body { get; set; }
		public string Root { get; set; }
		public string Abdomen { get; set; }
		public string Throw { get; set; }
		public string Waist { get; set; }
		public string SpineA { get; set; }
		public string LegLeft { get; set; }
		public string LegRight { get; set; }
		public string HolsterLeft { get; set; }
		public string HolsterRight { get; set; }
		public string SheatheLeft { get; set; }
		public string SheatheRight { get; set; }
		public string SpineB { get; set; }
		public string ClothBackALeft { get; set; }
		public string ClothBackARight { get; set; }
		public string ClothFrontALeft { get; set; }
		public string ClothFrontARight { get; set; }
		public string ClothSideALeft { get; set; }
		public string ClothSideARight { get; set; }
		public string KneeLeft { get; set; }
		public string KneeRight { get; set; }
		public string BreastLeft { get; set; }
		public string BreastRight { get; set; }
		public string SpineC { get; set; }
		public string ClothBackBLeft { get; set; }
		public string ClothBackBRight { get; set; }
		public string ClothFrontBLeft { get; set; }
		public string ClothFrontBRight { get; set; }
		public string ClothSideBLeft { get; set; }
		public string ClothSideBRight { get; set; }
		public string CalfLeft { get; set; }
		public string CalfRight { get; set; }
		public string ScabbardLeft { get; set; }
		public string ScabbardRight { get; set; }
		public string Neck { get; set; }
		public string ClavicleLeft { get; set; }
		public string ClavicleRight { get; set; }
		public string ClothBackCLeft { get; set; }
		public string ClothBackCRight { get; set; }
		public string ClothFrontCLeft { get; set; }
		public string ClothFrontCRight { get; set; }
		public string ClothSideCLeft { get; set; }
		public string ClothSideCRight { get; set; }
		public string PoleynLeft { get; set; }
		public string PoleynRight { get; set; }
		public string FootLeft { get; set; }
		public string FootRight { get; set; }
		public string Head { get; set; }
		public string ArmLeft { get; set; }
		public string ArmRight { get; set; }
		public string PauldronLeft { get; set; }
		public string PauldronRight { get; set; }
		public string Unknown00 { get; set; }
		public string ToesLeft { get; set; }
		public string ToesRight { get; set; }
		public string HairA { get; set; }
		public string HairFrontLeft { get; set; }
		public string HairFrontRight { get; set; }
		public string EarLeft { get; set; }
		public string EarRight { get; set; }
		public string ForearmLeft { get; set; }
		public string ForearmRight { get; set; }
		public string ShoulderLeft { get; set; }
		public string ShoulderRight { get; set; }
		public string HairB { get; set; }
		public string HandLeft { get; set; }
		public string HandRight { get; set; }
		public string ShieldLeft { get; set; }
		public string ShieldRight { get; set; }
		public string EarringALeft { get; set; }
		public string EarringARight { get; set; }
		public string ElbowLeft { get; set; }
		public string ElbowRight { get; set; }
		public string CouterLeft { get; set; }
		public string CouterRight { get; set; }
		public string WristLeft { get; set; }
		public string WristRight { get; set; }
		public string IndexALeft { get; set; }
		public string IndexARight { get; set; }
		public string PinkyALeft { get; set; }
		public string PinkyARight { get; set; }
		public string RingALeft { get; set; }
		public string RingARight { get; set; }
		public string MiddleALeft { get; set; }
		public string MiddleARight { get; set; }
		public string ThumbALeft { get; set; }
		public string ThumbARight { get; set; }
		public string WeaponLeft { get; set; }
		public string WeaponRight { get; set; }
		public string EarringBLeft { get; set; }
		public string EarringBRight { get; set; }
		public string IndexBLeft { get; set; }
		public string IndexBRight { get; set; }
		public string PinkyBLeft { get; set; }
		public string PinkyBRight { get; set; }
		public string RingBLeft { get; set; }
		public string RingBRight { get; set; }
		public string MiddleBLeft { get; set; }
		public string MiddleBRight { get; set; }
		public string ThumbBLeft { get; set; }
		public string ThumbBRight { get; set; }
		public string TailA { get; set; }
		public string TailB { get; set; }
		public string TailC { get; set; }
		public string TailD { get; set; }
		public string TailE { get; set; }
		public string RootHead { get; set; }
		public string Jaw { get; set; }
		public string EyelidLowerLeft { get; set; }
		public string EyelidLowerRight { get; set; }
		public string EyeLeft { get; set; }
		public string EyeRight { get; set; }
		public string Nose { get; set; }
		public string CheekLeft { get; set; }
		public string HrothWhiskersLeft { get; set; }
		public string CheekRight { get; set; }
		public string HrothWhiskersRight { get; set; }
		public string LipsLeft { get; set; }
		public string HrothEyebrowLeft { get; set; }
		public string LipsRight { get; set; }
		public string HrothEyebrowRight { get; set; }
		public string EyebrowLeft { get; set; }
		public string HrothBridge { get; set; }
		public string EyebrowRight { get; set; }
		public string HrothBrowLeft { get; set; }
		public string Bridge { get; set; }
		public string HrothBrowRight { get; set; }
		public string BrowLeft { get; set; }
		public string HrothJawUpper { get; set; }
		public string BrowRight { get; set; }
		public string HrothLipUpper { get; set; }
		public string LipUpperA { get; set; }
		public string HrothEyelidUpperLeft { get; set; }
		public string EyelidUpperLeft { get; set; }
		public string HrothEyelidUpperRight { get; set; }
		public string EyelidUpperRight { get; set; }
		public string HrothLipsLeft { get; set; }
		public string LipLowerA { get; set; }
		public string HrothLipsRight { get; set; }
		public string VieraEar01ALeft { get; set; }
		public string LipUpperB { get; set; }
		public string HrothLipUpperLeft { get; set; }
		public string VieraEar01ARight { get; set; }
		public string LipLowerB { get; set; }
		public string HrothLipUpperRight { get; set; }
		public string VieraEar02ALeft { get; set; }
		public string HrothLipLower { get; set; }
		public string VieraEar02ARight { get; set; }
		public string VieraEar03ALeft { get; set; }
		public string VieraEar03ARight { get; set; }
		public string VieraEar04ALeft { get; set; }
		public string VieraEar04ARight { get; set; }
		public string VieraLipLowerA { get; set; }
		public string VieraLipUpperB { get; set; }
		public string VieraEar01BLeft { get; set; }
		public string VieraEar01BRight { get; set; }
		public string VieraEar02BLeft { get; set; }
		public string VieraEar02BRight { get; set; }
		public string VieraEar03BLeft { get; set; }
		public string VieraEar03BRight { get; set; }
		public string VieraEar04BLeft { get; set; }
		public string VieraEar04BRight { get; set; }
		public string VieraLipLowerB { get; set; }
		public string ExRootHair { get; set; }
		public string ExHairA { get; set; }
		public string ExHairB { get; set; }
		public string ExHairC { get; set; }
		public string ExHairD { get; set; }
		public string ExHairE { get; set; }
		public string ExHairF { get; set; }
		public string ExHairG { get; set; }
		public string ExHairH { get; set; }
		public string ExHairI { get; set; }
		public string ExHairJ { get; set; }
		public string ExHairK { get; set; }
		public string ExHairL { get; set; }
		public string ExRootMet { get; set; }
		public string ExMetA { get; set; }
		public string ExMetB { get; set; }
		public string ExMetC { get; set; }
		public string ExMetD { get; set; }
		public string ExMetE { get; set; }
		public string ExMetF { get; set; }
		public string ExMetG { get; set; }
		public string ExMetH { get; set; }
		public string ExMetI { get; set; }
		public string ExMetJ { get; set; }
		public string ExMetK { get; set; }
		public string ExMetL { get; set; }
		public string ExMetM { get; set; }
		public string ExMetN { get; set; }
		public string ExMetO { get; set; }
		public string ExMetP { get; set; }
		public string ExMetQ { get; set; }
		public string ExMetR { get; set; }
		public string ExRootTop { get; set; }
		public string ExTopA { get; set; }
		public string ExTopB { get; set; }
		public string ExTopC { get; set; }
		public string ExTopD { get; set; }
		public string ExTopE { get; set; }
		public string ExTopF { get; set; }
		public string ExTopG { get; set; }
		public string ExTopH { get; set; }
		public string ExTopI { get; set; }

		public override FileType GetFileType()
		{
			return FileType;
		}

		public PoseFile Upgrade()
		{
			PoseFile file = new PoseFile();
			file.Description = this.Description;

			file.Race = int.Parse(this.Race);
			file.Clan = int.Parse(this.Clan);
			file.Body = int.Parse(this.Body);

			file.Root = StringToBone(this.Root);
			file.Abdomen = StringToBone(this.Abdomen);
			file.Throw = StringToBone(this.Throw);
			file.Waist = StringToBone(this.Waist);
			file.SpineA = StringToBone(this.SpineA);
			file.LegLeft = StringToBone(this.LegLeft);
			file.LegRight = StringToBone(this.LegRight);
			file.HolsterLeft = StringToBone(this.HolsterLeft);
			file.HolsterRight = StringToBone(this.HolsterRight);
			file.SheatheLeft = StringToBone(this.SheatheLeft);
			file.SheatheRight = StringToBone(this.SheatheRight);
			file.SpineB = StringToBone(this.SpineB);
			file.ClothBackALeft = StringToBone(this.ClothBackALeft);
			file.ClothBackARight = StringToBone(this.ClothBackARight);
			file.ClothFrontALeft = StringToBone(this.ClothFrontALeft);
			file.ClothFrontARight = StringToBone(this.ClothFrontARight);
			file.ClothSideALeft = StringToBone(this.ClothSideALeft);
			file.ClothSideARight = StringToBone(this.ClothSideARight);
			file.KneeLeft = StringToBone(this.KneeLeft);
			file.KneeRight = StringToBone(this.KneeRight);
			file.BreastLeft = StringToBone(this.BreastLeft);
			file.BreastRight = StringToBone(this.BreastRight);
			file.SpineC = StringToBone(this.SpineC);
			file.ClothBackBLeft = StringToBone(this.ClothBackBLeft);
			file.ClothBackBRight = StringToBone(this.ClothBackBRight);
			file.ClothFrontBLeft = StringToBone(this.ClothFrontBLeft);
			file.ClothFrontBRight = StringToBone(this.ClothFrontBRight);
			file.ClothSideBLeft = StringToBone(this.ClothSideBLeft);
			file.ClothSideBRight = StringToBone(this.ClothSideBRight);
			file.CalfLeft = StringToBone(this.CalfLeft);
			file.CalfRight = StringToBone(this.CalfRight);
			file.ScabbardLeft = StringToBone(this.ScabbardLeft);
			file.ScabbardRight = StringToBone(this.ScabbardRight);
			file.Neck = StringToBone(this.Neck);
			file.ClavicleLeft = StringToBone(this.ClavicleLeft);
			file.ClavicleRight = StringToBone(this.ClavicleRight);
			file.ClothBackCLeft = StringToBone(this.ClothBackCLeft);
			file.ClothBackCRight = StringToBone(this.ClothBackCRight);
			file.ClothFrontCLeft = StringToBone(this.ClothFrontCLeft);
			file.ClothFrontCRight = StringToBone(this.ClothFrontCRight);
			file.ClothSideCLeft = StringToBone(this.ClothSideCLeft);
			file.ClothSideCRight = StringToBone(this.ClothSideCRight);
			file.PoleynLeft = StringToBone(this.PoleynLeft);
			file.PoleynRight = StringToBone(this.PoleynRight);
			file.FootLeft = StringToBone(this.FootLeft);
			file.FootRight = StringToBone(this.FootRight);
			file.Head = StringToBone(this.Head);
			file.ArmLeft = StringToBone(this.ArmLeft);
			file.ArmRight = StringToBone(this.ArmRight);
			file.PauldronLeft = StringToBone(this.PauldronLeft);
			file.PauldronRight = StringToBone(this.PauldronRight);
			file.Unknown00 = StringToBone(this.Unknown00);
			file.ToesLeft = StringToBone(this.ToesLeft);
			file.ToesRight = StringToBone(this.ToesRight);
			file.HairA = StringToBone(this.HairA);
			file.HairFrontLeft = StringToBone(this.HairFrontLeft);
			file.HairFrontRight = StringToBone(this.HairFrontRight);
			file.EarLeft = StringToBone(this.EarLeft);
			file.EarRight = StringToBone(this.EarRight);
			file.ForearmLeft = StringToBone(this.ForearmLeft);
			file.ForearmRight = StringToBone(this.ForearmRight);
			file.ShoulderLeft = StringToBone(this.ShoulderLeft);
			file.ShoulderRight = StringToBone(this.ShoulderRight);
			file.HairB = StringToBone(this.HairB);
			file.HandLeft = StringToBone(this.HandLeft);
			file.HandRight = StringToBone(this.HandRight);
			file.ShieldLeft = StringToBone(this.ShieldLeft);
			file.ShieldRight = StringToBone(this.ShieldRight);
			file.EarringALeft = StringToBone(this.EarringALeft);
			file.EarringARight = StringToBone(this.EarringARight);
			file.ElbowLeft = StringToBone(this.ElbowLeft);
			file.ElbowRight = StringToBone(this.ElbowRight);
			file.CouterLeft = StringToBone(this.CouterLeft);
			file.CouterRight = StringToBone(this.CouterRight);
			file.WristLeft = StringToBone(this.WristLeft);
			file.WristRight = StringToBone(this.WristRight);
			file.IndexALeft = StringToBone(this.IndexALeft);
			file.IndexARight = StringToBone(this.IndexARight);
			file.PinkyALeft = StringToBone(this.PinkyALeft);
			file.PinkyARight = StringToBone(this.PinkyARight);
			file.RingALeft = StringToBone(this.RingALeft);
			file.RingARight = StringToBone(this.RingARight);
			file.MiddleALeft = StringToBone(this.MiddleALeft);
			file.MiddleARight = StringToBone(this.MiddleARight);
			file.ThumbALeft = StringToBone(this.ThumbALeft);
			file.ThumbARight = StringToBone(this.ThumbARight);
			file.WeaponLeft = StringToBone(this.WeaponLeft);
			file.WeaponRight = StringToBone(this.WeaponRight);
			file.EarringBLeft = StringToBone(this.EarringBLeft);
			file.EarringBRight = StringToBone(this.EarringBRight);
			file.IndexBLeft = StringToBone(this.IndexBLeft);
			file.IndexBRight = StringToBone(this.IndexBRight);
			file.PinkyBLeft = StringToBone(this.PinkyBLeft);
			file.PinkyBRight = StringToBone(this.PinkyBRight);
			file.RingBLeft = StringToBone(this.RingBLeft);
			file.RingBRight = StringToBone(this.RingBRight);
			file.MiddleBLeft = StringToBone(this.MiddleBLeft);
			file.MiddleBRight = StringToBone(this.MiddleBRight);
			file.ThumbBLeft = StringToBone(this.ThumbBLeft);
			file.ThumbBRight = StringToBone(this.ThumbBRight);
			file.TailA = StringToBone(this.TailA);
			file.TailB = StringToBone(this.TailB);
			file.TailC = StringToBone(this.TailC);
			file.TailD = StringToBone(this.TailD);
			file.TailE = StringToBone(this.TailE);
			file.RootHead = StringToBone(this.RootHead);
			file.Jaw = StringToBone(this.Jaw);
			file.EyelidLowerLeft = StringToBone(this.EyelidLowerLeft);
			file.EyelidLowerRight = StringToBone(this.EyelidLowerRight);
			file.EyeLeft = StringToBone(this.EyeLeft);
			file.EyeRight = StringToBone(this.EyeRight);
			file.Nose = StringToBone(this.Nose);
			file.CheekLeft = StringToBone(this.CheekLeft);
			file.HrothWhiskersLeft = StringToBone(this.HrothWhiskersLeft);
			file.CheekRight = StringToBone(this.CheekRight);
			file.HrothWhiskersRight = StringToBone(this.HrothWhiskersRight);
			file.LipsLeft = StringToBone(this.LipsLeft);
			file.HrothEyebrowLeft = StringToBone(this.HrothEyebrowLeft);
			file.LipsRight = StringToBone(this.LipsRight);
			file.HrothEyebrowRight = StringToBone(this.HrothEyebrowRight);
			file.EyebrowLeft = StringToBone(this.EyebrowLeft);
			file.HrothBridge = StringToBone(this.HrothBridge);
			file.EyebrowRight = StringToBone(this.EyebrowRight);
			file.HrothBrowLeft = StringToBone(this.HrothBrowLeft);
			file.Bridge = StringToBone(this.Bridge);
			file.HrothBrowRight = StringToBone(this.HrothBrowRight);
			file.BrowLeft = StringToBone(this.BrowLeft);
			file.HrothJawUpper = StringToBone(this.HrothJawUpper);
			file.BrowRight = StringToBone(this.BrowRight);
			file.HrothLipUpper = StringToBone(this.HrothLipUpper);
			file.LipUpperA = StringToBone(this.LipUpperA);
			file.HrothEyelidUpperLeft = StringToBone(this.HrothEyelidUpperLeft);
			file.EyelidUpperLeft = StringToBone(this.EyelidUpperLeft);
			file.HrothEyelidUpperRight = StringToBone(this.HrothEyelidUpperRight);
			file.EyelidUpperRight = StringToBone(this.EyelidUpperRight);
			file.HrothLipsLeft = StringToBone(this.HrothLipsLeft);
			file.LipLowerA = StringToBone(this.LipLowerA);
			file.HrothLipsRight = StringToBone(this.HrothLipsRight);
			file.VieraEar01ALeft = StringToBone(this.VieraEar01ALeft);
			file.LipUpperB = StringToBone(this.LipUpperB);
			file.HrothLipUpperLeft = StringToBone(this.HrothLipUpperLeft);
			file.VieraEar01ARight = StringToBone(this.VieraEar01ARight);
			file.LipLowerB = StringToBone(this.LipLowerB);
			file.HrothLipUpperRight = StringToBone(this.HrothLipUpperRight);
			file.VieraEar02ALeft = StringToBone(this.VieraEar02ALeft);
			file.HrothLipLower = StringToBone(this.HrothLipLower);
			file.VieraEar02ARight = StringToBone(this.VieraEar02ARight);
			file.VieraEar03ALeft = StringToBone(this.VieraEar03ALeft);
			file.VieraEar03ARight = StringToBone(this.VieraEar03ARight);
			file.VieraEar04ALeft = StringToBone(this.VieraEar04ALeft);
			file.VieraEar04ARight = StringToBone(this.VieraEar04ARight);
			file.VieraLipLowerA = StringToBone(this.VieraLipLowerA);
			file.VieraLipUpperB = StringToBone(this.VieraLipUpperB);
			file.VieraEar01BLeft = StringToBone(this.VieraEar01BLeft);
			file.VieraEar01BRight = StringToBone(this.VieraEar01BRight);
			file.VieraEar02BLeft = StringToBone(this.VieraEar02BLeft);
			file.VieraEar02BRight = StringToBone(this.VieraEar02BRight);
			file.VieraEar03BLeft = StringToBone(this.VieraEar03BLeft);
			file.VieraEar03BRight = StringToBone(this.VieraEar03BRight);
			file.VieraEar04BLeft = StringToBone(this.VieraEar04BLeft);
			file.VieraEar04BRight = StringToBone(this.VieraEar04BRight);
			file.VieraLipLowerB = StringToBone(this.VieraLipLowerB);
			file.ExRootHair = StringToBone(this.ExRootHair);
			file.ExHairA = StringToBone(this.ExHairA);
			file.ExHairB = StringToBone(this.ExHairB);
			file.ExHairC = StringToBone(this.ExHairC);
			file.ExHairD = StringToBone(this.ExHairD);
			file.ExHairE = StringToBone(this.ExHairE);
			file.ExHairF = StringToBone(this.ExHairF);
			file.ExHairG = StringToBone(this.ExHairG);
			file.ExHairH = StringToBone(this.ExHairH);
			file.ExHairI = StringToBone(this.ExHairI);
			file.ExHairJ = StringToBone(this.ExHairJ);
			file.ExHairK = StringToBone(this.ExHairK);
			file.ExHairL = StringToBone(this.ExHairL);
			file.ExRootMet = StringToBone(this.ExRootMet);
			file.ExMetA = StringToBone(this.ExMetA);
			file.ExMetB = StringToBone(this.ExMetB);
			file.ExMetC = StringToBone(this.ExMetC);
			file.ExMetD = StringToBone(this.ExMetD);
			file.ExMetE = StringToBone(this.ExMetE);
			file.ExMetF = StringToBone(this.ExMetF);
			file.ExMetG = StringToBone(this.ExMetG);
			file.ExMetH = StringToBone(this.ExMetH);
			file.ExMetI = StringToBone(this.ExMetI);
			file.ExMetJ = StringToBone(this.ExMetJ);
			file.ExMetK = StringToBone(this.ExMetK);
			file.ExMetL = StringToBone(this.ExMetL);
			file.ExMetM = StringToBone(this.ExMetM);
			file.ExMetN = StringToBone(this.ExMetN);
			file.ExMetO = StringToBone(this.ExMetO);
			file.ExMetP = StringToBone(this.ExMetP);
			file.ExMetQ = StringToBone(this.ExMetQ);
			file.ExMetR = StringToBone(this.ExMetR);
			file.ExRootTop = StringToBone(this.ExRootTop);
			file.ExTopA = StringToBone(this.ExTopA);
			file.ExTopB = StringToBone(this.ExTopB);
			file.ExTopC = StringToBone(this.ExTopC);
			file.ExTopD = StringToBone(this.ExTopD);
			file.ExTopE = StringToBone(this.ExTopE);
			file.ExTopF = StringToBone(this.ExTopF);
			file.ExTopG = StringToBone(this.ExTopG);
			file.ExTopH = StringToBone(this.ExTopH);
			file.ExTopI = StringToBone(this.ExTopI);

			return file;
		}

		private static PoseFile.Bone StringToBone(string input)
		{
			if (input == "null")
				return null;

			input = input.Replace(" ", string.Empty);
			byte[] data = StringToByteArray(input);

			PoseFile.Bone bone = new PoseFile.Bone();

			Quaternion value = default(Quaternion);
			value.X = BitConverter.ToSingle(data, 0);
			value.Y = BitConverter.ToSingle(data, 4);
			value.Z = BitConverter.ToSingle(data, 8);
			value.W = BitConverter.ToSingle(data, 12);
			bone.Rotation = value;

			return bone;
		}

		private static byte[] StringToByteArray(string hex)
		{
			int numChars = hex.Length;
			byte[] bytes = new byte[numChars / 2];

			for (int i = 0; i < numChars; i += 2)
			{
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}

			return bytes;
		}
	}
}
