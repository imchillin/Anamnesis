// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Services;

	public class PoseFile : FileBase
	{
		public static readonly FileType FileType = new FileType(@"cm3p", "CM3 Pose File", typeof(PoseFile));

		public string Description { get; set; }

		public int Race { get; set; }
		public int Clan { get; set; }
		public int Body { get; set; }

		public Bone Root { get; set; }
		public Bone Abdomen { get; set; }
		public Bone Throw { get; set; }
		public Bone Waist { get; set; }
		public Bone SpineA { get; set; }
		public Bone LegLeft { get; set; }
		public Bone LegRight { get; set; }
		public Bone HolsterLeft { get; set; }
		public Bone HolsterRight { get; set; }
		public Bone SheatheLeft { get; set; }
		public Bone SheatheRight { get; set; }
		public Bone SpineB { get; set; }
		public Bone ClothBackALeft { get; set; }
		public Bone ClothBackARight { get; set; }
		public Bone ClothFrontALeft { get; set; }
		public Bone ClothFrontARight { get; set; }
		public Bone ClothSideALeft { get; set; }
		public Bone ClothSideARight { get; set; }
		public Bone KneeLeft { get; set; }
		public Bone KneeRight { get; set; }
		public Bone BreastLeft { get; set; }
		public Bone BreastRight { get; set; }
		public Bone SpineC { get; set; }
		public Bone ClothBackBLeft { get; set; }
		public Bone ClothBackBRight { get; set; }
		public Bone ClothFrontBLeft { get; set; }
		public Bone ClothFrontBRight { get; set; }
		public Bone ClothSideBLeft { get; set; }
		public Bone ClothSideBRight { get; set; }
		public Bone CalfLeft { get; set; }
		public Bone CalfRight { get; set; }
		public Bone ScabbardLeft { get; set; }
		public Bone ScabbardRight { get; set; }
		public Bone Neck { get; set; }
		public Bone ClavicleLeft { get; set; }
		public Bone ClavicleRight { get; set; }
		public Bone ClothBackCLeft { get; set; }
		public Bone ClothBackCRight { get; set; }
		public Bone ClothFrontCLeft { get; set; }
		public Bone ClothFrontCRight { get; set; }
		public Bone ClothSideCLeft { get; set; }
		public Bone ClothSideCRight { get; set; }
		public Bone PoleynLeft { get; set; }
		public Bone PoleynRight { get; set; }
		public Bone FootLeft { get; set; }
		public Bone FootRight { get; set; }
		public Bone Head { get; set; }
		public Bone ArmLeft { get; set; }
		public Bone ArmRight { get; set; }
		public Bone PauldronLeft { get; set; }
		public Bone PauldronRight { get; set; }
		public Bone Unknown00 { get; set; }
		public Bone ToesLeft { get; set; }
		public Bone ToesRight { get; set; }
		public Bone HairA { get; set; }
		public Bone HairFrontLeft { get; set; }
		public Bone HairFrontRight { get; set; }
		public Bone EarLeft { get; set; }
		public Bone EarRight { get; set; }
		public Bone ForearmLeft { get; set; }
		public Bone ForearmRight { get; set; }
		public Bone ShoulderLeft { get; set; }
		public Bone ShoulderRight { get; set; }
		public Bone HairB { get; set; }
		public Bone HandLeft { get; set; }
		public Bone HandRight { get; set; }
		public Bone ShieldLeft { get; set; }
		public Bone ShieldRight { get; set; }
		public Bone EarringALeft { get; set; }
		public Bone EarringARight { get; set; }
		public Bone ElbowLeft { get; set; }
		public Bone ElbowRight { get; set; }
		public Bone CouterLeft { get; set; }
		public Bone CouterRight { get; set; }
		public Bone WristLeft { get; set; }
		public Bone WristRight { get; set; }
		public Bone IndexALeft { get; set; }
		public Bone IndexARight { get; set; }
		public Bone PinkyALeft { get; set; }
		public Bone PinkyARight { get; set; }
		public Bone RingALeft { get; set; }
		public Bone RingARight { get; set; }
		public Bone MiddleALeft { get; set; }
		public Bone MiddleARight { get; set; }
		public Bone ThumbALeft { get; set; }
		public Bone ThumbARight { get; set; }
		public Bone WeaponLeft { get; set; }
		public Bone WeaponRight { get; set; }
		public Bone EarringBLeft { get; set; }
		public Bone EarringBRight { get; set; }
		public Bone IndexBLeft { get; set; }
		public Bone IndexBRight { get; set; }
		public Bone PinkyBLeft { get; set; }
		public Bone PinkyBRight { get; set; }
		public Bone RingBLeft { get; set; }
		public Bone RingBRight { get; set; }
		public Bone MiddleBLeft { get; set; }
		public Bone MiddleBRight { get; set; }
		public Bone ThumbBLeft { get; set; }
		public Bone ThumbBRight { get; set; }
		public Bone TailA { get; set; }
		public Bone TailB { get; set; }
		public Bone TailC { get; set; }
		public Bone TailD { get; set; }
		public Bone TailE { get; set; }
		public Bone RootHead { get; set; }
		public Bone Jaw { get; set; }
		public Bone EyelidLowerLeft { get; set; }
		public Bone EyelidLowerRight { get; set; }
		public Bone EyeLeft { get; set; }
		public Bone EyeRight { get; set; }
		public Bone Nose { get; set; }
		public Bone CheekLeft { get; set; }
		public Bone HrothWhiskersLeft { get; set; }
		public Bone CheekRight { get; set; }
		public Bone HrothWhiskersRight { get; set; }
		public Bone LipsLeft { get; set; }
		public Bone HrothEyebrowLeft { get; set; }
		public Bone LipsRight { get; set; }
		public Bone HrothEyebrowRight { get; set; }
		public Bone EyebrowLeft { get; set; }
		public Bone HrothBridge { get; set; }
		public Bone EyebrowRight { get; set; }
		public Bone HrothBrowLeft { get; set; }
		public Bone Bridge { get; set; }
		public Bone HrothBrowRight { get; set; }
		public Bone BrowLeft { get; set; }
		public Bone HrothJawUpper { get; set; }
		public Bone BrowRight { get; set; }
		public Bone HrothLipUpper { get; set; }
		public Bone LipUpperA { get; set; }
		public Bone HrothEyelidUpperLeft { get; set; }
		public Bone EyelidUpperLeft { get; set; }
		public Bone HrothEyelidUpperRight { get; set; }
		public Bone EyelidUpperRight { get; set; }
		public Bone HrothLipsLeft { get; set; }
		public Bone LipLowerA { get; set; }
		public Bone HrothLipsRight { get; set; }
		public Bone VieraEar01ALeft { get; set; }
		public Bone LipUpperB { get; set; }
		public Bone HrothLipUpperLeft { get; set; }
		public Bone VieraEar01ARight { get; set; }
		public Bone LipLowerB { get; set; }
		public Bone HrothLipUpperRight { get; set; }
		public Bone VieraEar02ALeft { get; set; }
		public Bone HrothLipLower { get; set; }
		public Bone VieraEar02ARight { get; set; }
		public Bone VieraEar03ALeft { get; set; }
		public Bone VieraEar03ARight { get; set; }
		public Bone VieraEar04ALeft { get; set; }
		public Bone VieraEar04ARight { get; set; }
		public Bone VieraLipLowerA { get; set; }
		public Bone VieraLipUpperB { get; set; }
		public Bone VieraEar01BLeft { get; set; }
		public Bone VieraEar01BRight { get; set; }
		public Bone VieraEar02BLeft { get; set; }
		public Bone VieraEar02BRight { get; set; }
		public Bone VieraEar03BLeft { get; set; }
		public Bone VieraEar03BRight { get; set; }
		public Bone VieraEar04BLeft { get; set; }
		public Bone VieraEar04BRight { get; set; }
		public Bone VieraLipLowerB { get; set; }
		public Bone ExRootHair { get; set; }
		public Bone ExHairA { get; set; }
		public Bone ExHairB { get; set; }
		public Bone ExHairC { get; set; }
		public Bone ExHairD { get; set; }
		public Bone ExHairE { get; set; }
		public Bone ExHairF { get; set; }
		public Bone ExHairG { get; set; }
		public Bone ExHairH { get; set; }
		public Bone ExHairI { get; set; }
		public Bone ExHairJ { get; set; }
		public Bone ExHairK { get; set; }
		public Bone ExHairL { get; set; }
		public Bone ExRootMet { get; set; }
		public Bone ExMetA { get; set; }
		public Bone ExMetB { get; set; }
		public Bone ExMetC { get; set; }
		public Bone ExMetD { get; set; }
		public Bone ExMetE { get; set; }
		public Bone ExMetF { get; set; }
		public Bone ExMetG { get; set; }
		public Bone ExMetH { get; set; }
		public Bone ExMetI { get; set; }
		public Bone ExMetJ { get; set; }
		public Bone ExMetK { get; set; }
		public Bone ExMetL { get; set; }
		public Bone ExMetM { get; set; }
		public Bone ExMetN { get; set; }
		public Bone ExMetO { get; set; }
		public Bone ExMetP { get; set; }
		public Bone ExMetQ { get; set; }
		public Bone ExMetR { get; set; }
		public Bone ExRootTop { get; set; }
		public Bone ExTopA { get; set; }
		public Bone ExTopB { get; set; }
		public Bone ExTopC { get; set; }
		public Bone ExTopD { get; set; }
		public Bone ExTopE { get; set; }
		public Bone ExTopF { get; set; }
		public Bone ExTopG { get; set; }
		public Bone ExTopH { get; set; }
		public Bone ExTopI { get; set; }

		public override FileType GetFileType()
		{
			return FileType;
		}

		[Serializable]
		public class Bone
		{
			public Quaternion Rotation;

			// soon
			////public Vector3D Scale;
		}
	}
}
